namespace Aml.Application.Audit
{
    public interface IAuditService
    {
        Task LogAsync(
            string userName, 
            AuditAction action, 
            string entityType,
            int entityId,
            string? oldValue,
            string? newValue,
            CancellationToken ct);
    }
}
