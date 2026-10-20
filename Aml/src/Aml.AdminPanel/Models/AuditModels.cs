namespace Aml.AdminPanel.Models;

public record AuditLogItem(
    int Id, 
    string UserName,
    string Action,
    string EntityType,
    int EntityId,
    string? OldValue,
    string? NewValue,
    DateTime CreatedAt);

public record AuditListResponse(
    int Total,
    int Page, 
    int PageSize,
    AuditLogItem[] Items);
