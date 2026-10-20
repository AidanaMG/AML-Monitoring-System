using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    public class CryptoArbitrageRule : ITransactionRule
    {
        public string Code => "CRYPTO";
        public RuleType Type => RuleType.Typology;


        public RuleResult Evaluate(RuleContext context)
        {
            List<string> triggeredChecks = new();
            int matchedCount = 0;
            int contribution = 0;
            string details = "";

            var typologyConfidenceScoreThreshold = context.Settings.Get(Code, "typologyConfidenceScoreThreshold",
                ScreeningThresholds.TypologyConfidenceScoreThreshold);
            var unevenAmountRatioMinRange = context.Settings.Get(Code, "unevenAmountRatioMinRange",
                CryptoArbitrageThresholds.UnevenAmountRatioMinRange);
            var transactionsForRatioCheckMinRange = context.Settings.Get(Code, "transactionsForRatioCheckMinRange",
                CryptoArbitrageThresholds.TransactionsForRatioCheckMinRange);
            var p2pAndIntrabankCountLast30DaysMinRange = context.Settings.Get(Code, "p2pAndIntrabankCountLast30DaysMinRange",
                CryptoArbitrageThresholds.P2pAndIntrabankCountLast30DaysMinRange);
            var uniqueCounterpartyCountLast30DaysMinRange = context.Settings.Get(Code, "uniqueCounterpartyCountLast30DaysMinRange",
                CryptoArbitrageThresholds.UniqueCounterpartyCountLast30DaysMinRange);
            var nonResidentCounterpartyCountLast30DaysMinRange = context.Settings.Get(Code, "nonResidentCounterpartyCountLast30DaysMinRange",
                CryptoArbitrageThresholds.NonResidentCounterpartyCountLast30DaysMinRange);
            var nonAifcCryptoExchangeTransactionCountLast30DaysMinRange = context.Settings.Get(Code, "nonAifcCryptoExchangeTransactionCountLast30DaysMinRange",
                CryptoArbitrageThresholds.NonAifcCryptoExchangeTransactionCountLast30DaysMinRange);


            // #1
            var p2pAndIntrabankAmountsLast30Days = context.ClientActivity.P2pAndIntrabankAmountsLast30Days;
            int unevenAmountCounter = 0;

            foreach(decimal amount in p2pAndIntrabankAmountsLast30Days)
            {
                if(amount % 100 != 0)
                {
                    unevenAmountCounter++;
                }
            }

            if(p2pAndIntrabankAmountsLast30Days.Count >= transactionsForRatioCheckMinRange
                && unevenAmountCounter >= context.ClientActivity.P2pAndIntrabankTransactionCountLast30Days * unevenAmountRatioMinRange) 
            {
                matchedCount++;
                contribution += 15; 

                triggeredChecks.Add($"Transactions (P2P and Intrabank transfers) with uneven amounts count last 30 days: {(int)unevenAmountCounter} [min range: {(int)(context.ClientActivity.P2pAndIntrabankTransactionCountLast30Days * unevenAmountRatioMinRange)}]"); 
            }


            // #2 
            if(context.ClientActivity.P2pAndIntrabankTransactionCountLast30Days >= p2pAndIntrabankCountLast30DaysMinRange) 
            {
                matchedCount++;
                contribution += 20; 

                triggeredChecks.Add($"Transactions (P2P and Intrabank transfers) count last 30 days: {context.ClientActivity.P2pAndIntrabankTransactionCountLast30Days} [min range: {p2pAndIntrabankCountLast30DaysMinRange}]");
            }


            // #3 
            if(context.ClientActivity.UniqueCounterpartyCountLast30Days >= uniqueCounterpartyCountLast30DaysMinRange) 
            {
                matchedCount++;
                contribution += 20; 

                triggeredChecks.Add($"Unique counterparty count last 30 days: {context.ClientActivity.UniqueCounterpartyCountLast30Days} [min range: {uniqueCounterpartyCountLast30DaysMinRange}]");
            }


            // #4
            if(context.ClientActivity.NonResidentCounterpartyCountLast30Days >= nonResidentCounterpartyCountLast30DaysMinRange) 
            {
                matchedCount++;
                contribution += 20; 

                triggeredChecks.Add($"Non-resident counterparty count last 30 days: {context.ClientActivity.NonResidentCounterpartyCountLast30Days} [min range: {nonResidentCounterpartyCountLast30DaysMinRange}]");
            
            }



            // #5  
            if(context.ClientActivity.NonAifcCryptoExchangeTransactionCountLast30Days >= nonAifcCryptoExchangeTransactionCountLast30DaysMinRange) 
            {
                matchedCount++;
                contribution += 25; 

                triggeredChecks.Add($"Non-aifc crypto exchange count last 30 days: {context.ClientActivity.NonAifcCryptoExchangeTransactionCountLast30Days} [min range: {nonAifcCryptoExchangeTransactionCountLast30DaysMinRange}]");
            }


            bool isTriggered = contribution >= typologyConfidenceScoreThreshold;


            if(triggeredChecks.Count > 0)
            {
                details = $"Triggered checks:\n- {string.Join("\n- ", triggeredChecks)}";

            }
            else
            {
                details = "Triggered checks: none";
            }



            return new RuleResult(isTriggered, matchedCount, CryptoArbitrageThresholds.TotalChecks, contribution, details);

        }
    }
}



