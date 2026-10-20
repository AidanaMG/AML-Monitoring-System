using Aml.Application.Audit;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aml.Api.Dtos;
using Aml.Application;

namespace Aml.Api.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly AmlDbContext _db;
        private readonly IAuditService _audit;
        private readonly IReferenceDataCache _cache;

        public AlertsController(AmlDbContext db, IAuditService audit, IReferenceDataCache cache)
        {
            _db = db;
            _audit = audit;
            _cache = cache;
        }


        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] AlertStatus[]? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var query = _db.Alerts.AsNoTracking();

            if(status is { Length: > 0 })
            {
                query = query.Where(q => status.Contains(q.Status));
            }

            var total = await query.CountAsync(ct); 

            var items = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize) 
                .Take(pageSize) 
                .Select(q => new AlertListItemDto(
                    q.Id, q.ClientId, q.TransactionId, 
                    q.PrimaryTrigger, 
                    q.RiskScore,
                    q.Status.ToString(),
                    q.CreatedAt))
                .ToListAsync(ct);

            return Ok(new { total, page, pageSize, items});
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var alert = await _db.Alerts.AsNoTracking()
                .Where(a => a.Id == id)
                .Join(_db.Clients, a => a.ClientId, c => c.Id, (a, c) => new
                    AlertDetailsDto(
                        a.Id, a.ClientId, c.Name + " " + c.Surname, a.TransactionId,
                        a.PrimaryTrigger, a.RiskScore, a.Status.ToString(),
                        a.TriggerDetails, a.ScoreBreakdownJson, a.CreatedAt, a.ResolvedAt, a.ResolvedBy, a.ResolutionComment, a.AssignedTo, null))
                .FirstOrDefaultAsync(ct);


            if(alert is null) return NotFound(); 

            if(alert.TransactionId is int transactionId)
            {
                var tx = await _db.Transactions.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == transactionId, ct);

                if(tx is not null)
                {
                    var transactionInfo = new TransactionInfoDto(
                        tx.Id,
                        tx.Type.ToString(),
                        tx.Direction.ToString(),
                        tx.Amount,
                        tx.Currency,
                        tx.TransactionDate,
                        tx.CounterpartyName,
                        tx.MerchantName,
                        tx.TransactionCountryId is int c1 ? _cache.GetCountry(c1)?.Name : null,
                        tx.CounterpartyCountryId is int c2 ? _cache.GetCountry(c2)?.Name : null);

                    alert = alert with { Transaction = transactionInfo };
                }
            }


            return Ok(alert);
        }


        [HttpPatch("{id:int}/resolve")]
        public async Task<IActionResult> Resolve(
            int id,
            [FromBody] ResolveAlertRequest request,
            CancellationToken ct)
        {
            if(request.Status != AlertStatus.Blocked && request.Status != AlertStatus.FalsePositive)
            {
                return BadRequest("Status must be Blocked or FalsePositive");
            }

            var userName = User.Identity?.Name;
            if(string.IsNullOrEmpty(userName)) return Unauthorized();

            var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);
            if(alert is null) return NotFound();

            if(alert.Status != AlertStatus.UnderReview)
                return Conflict($"Alert {id} must be taken to review first (current status: {alert.Status})");

            if(alert.AssignedTo != userName)
                return Conflict($"Alert {id} is assigned to {alert.AssignedTo}");

            var oldStatus = alert.Status;

            alert.Status = request.Status;
            alert.ResolutionComment = request.Comment;
            alert.ResolvedAt = DateTime.Now;
            alert.ResolvedBy = userName;


            if (request.Status == AlertStatus.FalsePositive)
            {
                var hasOtherOpenAlerts = await _db.Alerts
                    .AnyAsync(a => a.ClientId == alert.ClientId
                                && a.Id != alert.Id
                                && a.ResolvedAt == null, ct);

                var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == alert.ClientId, ct);
                var account = await _db.Accounts.FirstOrDefaultAsync(a => a.ClientId == alert.ClientId, ct);

                if(!hasOtherOpenAlerts)
                {
                    if (client is not null)
                    {
                        client.Status = ClientStatus.Active;
                        client.IsFlagged = false;
                    }

                    if(account is not null)
                    {
                        account.Status = AccountStatus.Active;
                    }

                    if (alert.TransactionId is int txId)
                    {
                        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == txId, ct);

                        if (transaction is not null)
                        {
                            transaction.Status = TransactionStatus.Completed;
                            transaction.IsFlagged = false;
                        }
                    }
                }
            }


                await _db.SaveChangesAsync(ct);

            await _audit.LogAsync(
                userName: userName,
                action: AuditAction.AlertResolved,
                entityType: nameof(Alert),
                entityId: alert.Id,
                oldValue: oldStatus.ToString(),
                newValue: $"status: {alert.Status}; comment: {request.Comment}",
                ct: ct);


            return Ok(new { alert.Id, status = alert.Status.ToString(), alert.ResolvedAt}); 
        }


        [HttpPatch("{id:int}/take")]
        public async Task<IActionResult> TakeToReview(int id, CancellationToken ct)
        {
            var userName = User.Identity?.Name;
            if(string.IsNullOrEmpty(userName)) return Unauthorized();

            var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);
            if(alert is null) return NotFound();

            if(alert.Status != AlertStatus.New)
                return Conflict($"Alert {id} is already in status {alert.Status}");

            alert.Status = AlertStatus.UnderReview;
            alert.AssignedTo = userName;

            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync(userName, AuditAction.AlertTakenToReview, nameof(Alert), alert.Id,
                oldValue: AlertStatus.New.ToString(), newValue: AlertStatus.UnderReview.ToString(), ct);

            return Ok(new { alert.Id, status = alert.Status.ToString(), alert.AssignedTo });

        }

    }
}

