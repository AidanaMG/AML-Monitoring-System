using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    public class CriticalRiskMerchantRule : ITransactionRule
    {
        public string Code => "CRITICAL_RISK_MERCHANT";
        public RuleType Type => RuleType.Hard;

        public RuleResult Evaluate(RuleContext context)
        {
            int matchedCounter = 0;
            string details = "";

            if(context.MerchantCategoryRiskLevel == RiskLevel.Critical)
            {
                matchedCounter++;
            }

            if (matchedCounter > 0)
            {
                details = $"Critical risk level merchant count: {matchedCounter}";
            }
            else
            {
                details = "Triggered checks: none";
            }


            return new RuleResult(matchedCounter > 0, matchedCounter, CriticalAndHighRiskThresholds.CriticalRiskMerchantTotalChecks, 0, details);

        }

    }
}
