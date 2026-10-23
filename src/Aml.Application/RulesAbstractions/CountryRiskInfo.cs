using Aml.Domain.Enums;


namespace Aml.Application.RulesAbstractions
{
    public record CountryRiskInfo(RiskLevel RiskLevel, bool FatfListed);
}
