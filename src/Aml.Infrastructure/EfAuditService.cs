using Aml.Application.Audit;
using Aml.Domain.Entities;

namespace Aml.Infrastructure
{
    public class EfAuditService : IAuditService
    {
        private readonly AmlDbContext _db;

        public EfAuditService(AmlDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(
            string userName,
            AuditAction action,
            string entityType,
            int entityId,
            string? oldValue,
            string? newValue,
            CancellationToken ct)
        {
            var log = new AuditLog { 
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.Now
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync(ct);
        }
    }
}
