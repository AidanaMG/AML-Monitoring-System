using Aml.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aml.Api.Dtos;

namespace Aml.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly AmlDbContext _db;
        public AuditController(AmlDbContext db) 
        { 
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] string? entityType,
            [FromQuery] int? entityId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var logQuery = _db.AuditLogs.AsNoTracking();

            if(entityId is not null)
            {
                logQuery = logQuery.Where(l => l.EntityId == entityId);
            }

            if(!string.IsNullOrWhiteSpace(entityType))
            {
                logQuery = logQuery.Where(l => l.EntityType == entityType);
            }

            var total = await logQuery.CountAsync(ct);

            var items = await logQuery
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogDto(
                    l.Id, l.UserName,
                    l.Action.ToString(), l.EntityType, l.EntityId,
                    l.OldValue, l.NewValue, l.CreatedAt))
                .ToListAsync(ct);


            return Ok(new { total, page, pageSize, items});
        }

    }
}
