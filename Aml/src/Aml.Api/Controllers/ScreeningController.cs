using Aml.Application;
using Aml.Application.Audit;
using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aml.Api.Controllers
{
    [ApiController]
    [Route("api/screening")]
    [Authorize]
    public class ScreeningController : ControllerBase
    {
        private readonly AmlDbContext _db;
        private readonly IRuleContextBuilder _builder;
        private readonly TransactionScreeningService _service;
        private readonly IAlertRepository _alerts;
        private readonly IAuditService _audit;


        public ScreeningController(
            AmlDbContext db,
            IRuleContextBuilder builder,
            TransactionScreeningService service,
            IAlertRepository alerts,
            IAuditService audit)
        {
            _db = db;
            _builder = builder;
            _service = service;
            _alerts = alerts;
            _audit = audit;
        }


        [HttpPost("{transactionId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Screen(int transactionId, CancellationToken ct)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var transaction = await _db.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == transactionId, ct);
            if(transaction is null) return NotFound();

            var context = await _builder.BuildAsync(transaction, transaction.TransactionDate, ct);

            var result = _service.Screening(context);


            int? alertId = null;

            if(result.ShouldBlock)
            {
                var activeAlert = await _db.Alerts.FirstOrDefaultAsync(
                a => a.TransactionId == transactionId && a.ResolvedAt == null, ct);

                var clientId = context.ClientFacts.ClientId;


                if (activeAlert is not null)
                {
                    alertId = activeAlert.Id;
                }
                else
                {
                    var alert = AlertFactory.Create(result, transaction, clientId, DateTime.Now);

                    if (alert is not null)
                    {
                        await _alerts.AddAsync(alert, ct);
                        alertId = alert.Id;
                    }
                }


                var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId, ct);
                if (client is not null)
                {
                    client.Status = ClientStatus.Blocked;
                    client.IsFlagged = true;
                }


                var accountToBlock = await _db.Accounts.FirstOrDefaultAsync(a => a.ClientId == clientId, ct);
                if(accountToBlock is not null)
                {
                    accountToBlock.Status = AccountStatus.Blocked;
                }

                var txToBlock = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId, ct);
                if (txToBlock is not null)
                {
                    txToBlock.Status = TransactionStatus.Blocked;
                    txToBlock.IsFlagged = true;
                }

                await _db.SaveChangesAsync(ct);

            }

            await _audit.LogAsync(
                userName: userName,
                action: AuditAction.ScreeningRun,
                entityType: nameof(Transaction),
                entityId: transactionId,
                oldValue: null,
                newValue: $"shouldBlock: {result.ShouldBlock}; score: {result.TotalScore}; alertId: {alertId}",
                ct: ct);


            string? blockReason = !result.ShouldBlock ? null
                : result.HardTriggered ? "Hard Rule"
                : result.TypologyTriggered ? "Typology Rule"
                : "Scoring rule";


            return Ok(new { 
                transactionId, 
                shouldBlock = result.ShouldBlock,
                reason = blockReason,
                totalScore = result.TotalScore,
                alertId,
                triggered = result.RuleEvaluations
                    .Where(e => e.Result.Triggered)
                    .Select(e => new { e. RuleCode, ruleType = e.RuleType.ToString(), e.Result.Contribution, e.Result.TriggerDetails})});
        }
    }
}

