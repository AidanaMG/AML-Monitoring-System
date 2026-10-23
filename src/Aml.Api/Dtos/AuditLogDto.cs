namespace Aml.Api.Dtos;
public record AuditLogDto(
    int Id,
    string UserName,
    string Action,
    string EntityType,
    int EntityId,
    string? OldValue,
    string? NewValue, 
    DateTime CreatedAt); 

