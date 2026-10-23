
using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    public class HighRiskMerchantRule : ITransactionRule
    {
        public string Code => "HIGH_RISK_MERCHANT";
        public RuleType Type => RuleType.Scoring;


        public RuleResult Evaluate(RuleContext context)
        {
            int matchedCounter = 0;
            bool isTriggered = false;
            int contribution = 0;
            string details = "";

            var highRiskMerchantCountLastHourMinRange = context.Settings.Get(Code, "highRiskMerchantCountLastHourMinRange",
                CriticalAndHighRiskThresholds.HighRiskMerchantCountLastHourMinRange);
            var weight = (int)context.Settings.GetWeight(Code,
                CriticalAndHighRiskThresholds.HighRiskMerchantScoringScoreThreshold);

            if (context.ClientActivity.HighRiskMerchantCountLastHour >= highRiskMerchantCountLastHourMinRange)
            {
                matchedCounter++;
                contribution = weight; 
                isTriggered = true;
            }

            if (isTriggered)
            {
                details = $"High risk level merchant count last hour: {context.ClientActivity.HighRiskMerchantCountLastHour} [min range: {highRiskMerchantCountLastHourMinRange}]";
            }
            else
            {
                details = "Triggered checks: none";
            }


            return new RuleResult(isTriggered, matchedCounter, CriticalAndHighRiskThresholds.HighRiskMerchantTotalChecks, contribution, details);

        }
    }
}
