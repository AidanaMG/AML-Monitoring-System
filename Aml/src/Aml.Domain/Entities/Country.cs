using Aml.Domain.Enums;

namespace Aml.Domain.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool FatfListed { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public CountryRiskTag? RiskDetails { get; set; } 
    }
}
