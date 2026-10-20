
using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    public class HighFrequencyRule : ITransactionRule
    {
        public string Code => "HIGH_FREQUENCY";
        public RuleType Type => RuleType.Scoring;


        public RuleResult Evaluate(RuleContext context)
        {
            int matchedCounter = 0;
            bool isTriggered = false;
            int contribution = 0;
            string details = "";

            var transactionCountLastHourMinRange = context.Settings.Get(Code, "transactionCountLastHourMinRange",
                CriticalAndHighRiskThresholds.TransactionCountLastHourMinRange);
            var weight = (int)context.Settings.GetWeight(Code,
                CriticalAndHighRiskThresholds.HighFrequencyScoringScoreThreshold);

            if(context.ClientActivity.TransactionCountLastHour >= transactionCountLastHourMinRange)
            {
                matchedCounter++;
                contribution = weight;
                isTriggered = true;
            }

            if(isTriggered)
            {
                details = $"High frequency pattern is detected | Transactions count last hour: {context.ClientActivity.TransactionCountLastHour} [min range: {transactionCountLastHourMinRange}]";
            }
            else
            {
                details = "Triggered checks: none";
            }

            return new RuleResult(isTriggered, matchedCounter, CriticalAndHighRiskThresholds.HighFrequencyTotalChecks, contribution, details);
        }

    }
}

