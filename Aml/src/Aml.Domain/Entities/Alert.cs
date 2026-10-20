using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class Alert
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? TransactionId { get; set; }
        public string TriggerDetails { get; set; } = null!;
        public decimal RiskScore { get; set; }
        public string PrimaryTrigger { get; set; } = null!; // e.g. "WATCHLIST", "DROPPER"
        public string ScoreBreakdownJson { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public AlertStatus Status { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionComment { get; set; }
        public string? AssignedTo { get; set; }
    }
}
