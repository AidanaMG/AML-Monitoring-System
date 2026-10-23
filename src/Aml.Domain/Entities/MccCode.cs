using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class MccCode
    {
        public int Id { get; set; }
        public string Mcc { get; set; } = null!;
        public string Category { get; set; } = null!;
        public RiskLevel RiskLevel { get; set; }
    }
}
