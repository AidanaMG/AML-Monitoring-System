
namespace Aml.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;   
        public AuditAction Action { get; set; } 
        public string EntityType { get; set; } = null!; 
        public int EntityId { get; set; }   
        public string? OldValue { get; set; }   
        public string? NewValue { get; set; }   
        public DateTime CreatedAt { get; set; }
    }
}
