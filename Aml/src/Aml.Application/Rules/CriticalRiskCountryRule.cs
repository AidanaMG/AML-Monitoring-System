using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;


namespace Aml.Application.Rules
{
    public class CriticalRiskCountryRule : ITransactionRule
    {
        public string Code => "CRITICAL_RISK_COUNTRY";
        public RuleType Type => RuleType.Hard;

        public RuleResult Evaluate(RuleContext context)
        {
            int matchedCounter = 0;
            string details = "";

            if(context.TransactionCountryRisk is not null && context.TransactionCountryRisk.RiskLevel == RiskLevel.Critical)
            {
                matchedCounter++;
            }


            if(context.CounterpartyCountryRisk is not null && context.CounterpartyCountryRisk.RiskLevel == RiskLevel.Critical)
            {
                matchedCounter++;
            }


            if(context.MerchantCountryRisk is not null && context.MerchantCountryRisk.RiskLevel == RiskLevel.Critical)
            {
                matchedCounter++;
            }

            if (matchedCounter > 0)
            {
                details = $"Critical risk level country count: {matchedCounter}";
            }
            else
            {
                details = "Triggered checks: none";
            }

            return new RuleResult(matchedCounter > 0, matchedCounter, CriticalAndHighRiskThresholds.CriticalRiskCountryTotalChecks, 0, details);

        }
    }
}
