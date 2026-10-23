using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;


namespace Aml.Application.Rules
{
    public class HighRiskCountryRule : ITransactionRule
    {
        public string Code => "HIGH_RISK_COUNTRY";
        public RuleType Type => RuleType.Scoring;


        public RuleResult Evaluate(RuleContext context)
        {
            int matchedCounter = 0;
            bool isTriggered = false;
            int contribution = 0;
            string details = "";

            var highRiskCountryTransactions3dMinRange = context.Settings.Get(Code, "highRiskCountryTransactions3dMinRange",
                CriticalAndHighRiskThresholds.HighRiskCountryTransactions3dMinRange);
            var weight = (int)context.Settings.GetWeight(Code,
                ScreeningThresholds.ScoringScoreThreshold);


            if (context.TransactionCountryRisk is not null && context.TransactionCountryRisk.RiskLevel == RiskLevel.High)
            {
                matchedCounter++;
            }


            if(context.CounterpartyCountryRisk is not null && context.CounterpartyCountryRisk.RiskLevel == RiskLevel.High)
            {
                matchedCounter++;
            }


            if(context.MerchantCountryRisk is not null && context.MerchantCountryRisk.RiskLevel == RiskLevel.High)
            {
                matchedCounter++;
            }


            if(matchedCounter > 0 && context.ClientActivity.TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days >= highRiskCountryTransactions3dMinRange)
            {
                contribution = weight; 
                isTriggered = true;
            }

            if (isTriggered)
            {
                details = $"High risk level country count in current transaction: {matchedCounter} [of 3 checked: operation, merchant, counterparty]" +
                $"\nTransactions count last 3 days in high risk level countries: {context.ClientActivity.TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days} [min range: {highRiskCountryTransactions3dMinRange}]";
            }
            else
            {
                details = "Triggered checks: none";
            }


            return new RuleResult(isTriggered, matchedCounter, CriticalAndHighRiskThresholds.HighRiskCountryTotalChecks, contribution, details);
        }
    }
}

