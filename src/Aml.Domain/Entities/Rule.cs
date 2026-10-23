using Aml.Domain.Enums;

namespace Aml.Domain.Entities
{
    public class Rule
    {
        public int Id { get; set; }
        public RuleType Type { get; set; }
        public RuleAction Action { get; set; }
        public string RuleCode { get; set; } = null!; // DROPPER, HIGH_FREQUENCY etc
        public string? Details { get; set; }
        public string ParametersJson { get; set; } = null!;
        public decimal Weight { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

