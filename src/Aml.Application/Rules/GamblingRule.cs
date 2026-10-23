using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;


namespace Aml.Application.Rules
{
    public class GamblingRule : ITransactionRule
    {
        public string Code => "GAMBLING";
        public RuleType Type => RuleType.Typology;


        public RuleResult Evaluate(RuleContext context)
        {
            List<string> triggeredChecks = new();
            int matchedCount = 0;
            int contribution = 0;
            string details = "";

            var typologyConfidenceScoreThreshold = context.Settings.Get(Code, "typologyConfidenceScoreThreshold",
                ScreeningThresholds.TypologyConfidenceScoreThreshold);
            var merchantCategoryCountLast24hMinRange = context.Settings.Get(Code, "merchantCategoryCountLast24hMinRange",
                GamblingThresholds.MerchantCategoryCountLast24hMinRange);
            var incomingFromGamblingMerchantsCountLast24hMinRange = context.Settings.Get(Code, "incomingFromGamblingMerchantsCountLast24hMinRange",
                GamblingThresholds.IncomingFromGamblingMerchantsCountLast24hMinRange);
            var retailTransactionCountLast30DaysMinRange = context.Settings.Get(Code, "retailTransactionCountLast30DaysMinRange",
                GamblingThresholds.RetailTransactionCountLast30DaysMinRange);
            var doubledAmountsTolerance = context.Settings.Get(Code, "doubledAmountsTolerance",
                GamblingThresholds.DoubledAmountsTolerance);
            var uniqueForeignMerchantTransactionCountLast24hMinRange = context.Settings.Get(Code, "uniqueForeignMerchantTransactionCountLast24hMinRange",
                GamblingThresholds.UniqueForeignMerchantTransactionCountLast24hMinRange);
            var minutesDifferenceBetweenTransactionsMaxRange = context.Settings.Get(Code, "minutesDifferenceBetweenTransactionsMaxRange",
                GamblingThresholds.MinutesDifferenceBetweenTransactionsMaxRange);
            var fractionalAmountCountMinRange = context.Settings.Get(Code, "fractionalAmountCountMinRange",
                GamblingThresholds.FractionalAmountCountMinRange);


            // #1
            if (context.ClientActivity.UniqueForeignMerchantTransactionCountLast24h >= uniqueForeignMerchantTransactionCountLast24hMinRange) 
            { 
                matchedCount++;
                contribution += 20;

                triggeredChecks.Add($"Unique foreign merchant count last 24h: {context.ClientActivity.UniqueForeignMerchantTransactionCountLast24h} [min range: {uniqueForeignMerchantTransactionCountLast24hMinRange}]");
            }

            // #2 + #3
            var transactions = context.ClientActivity.ForeignMerchantTransactionsLast24h.OrderBy(t => t.Time).ToList();

            if(transactions is not null && transactions.Count > 0)
            {
                int fractionalAmountCount = 0;
                bool hasAmountDoublingPatternWithinHour = false;

                for(int i = 0; i < transactions.Count; i++)
                {
                    if(transactions[i].Amount % 1m != 0)
                    {
                        fractionalAmountCount++;
                    }

                    if(i != transactions.Count - 1)
                    {
                        var currentTransaction = transactions[i];
                        var nextTransaction = transactions[i + 1];

                        bool isDoubled = Math.Abs(nextTransaction.Amount - currentTransaction.Amount * 2) <= doubledAmountsTolerance;

                        double minutesDiff = (nextTransaction.Time - currentTransaction.Time).TotalMinutes;

                        if(isDoubled && minutesDiff <= (double)minutesDifferenceBetweenTransactionsMaxRange) 
                        {
                            hasAmountDoublingPatternWithinHour = true;
                        }
                    }
                }

                if(fractionalAmountCount >= fractionalAmountCountMinRange)
                {
                    matchedCount++;
                    contribution += 25; 

                    triggeredChecks.Add($"Transactions with fractional amount count last 24h: {fractionalAmountCount} [min range: {fractionalAmountCountMinRange}]");
                }

                if(hasAmountDoublingPatternWithinHour)
                {
                    matchedCount++;
                    contribution += 15; 

                    triggeredChecks.Add($"Transactions with amount doubling pattern within an hour is detected");
                }
            }



            // #4
            if(context.ClientActivity.MerchantCategoryCountLast24h >= merchantCategoryCountLast24hMinRange)
            {
                matchedCount++;
                contribution += 20;

                triggeredChecks.Add($"Unique merchant category count last 24h: {context.ClientActivity.MerchantCategoryCountLast24h} [min range: {merchantCategoryCountLast24hMinRange}]");
            }


            // #5
            if(context.ClientActivity.RetailTransactionCountLast30Days <= retailTransactionCountLast30DaysMinRange)
            {
                matchedCount++;
                contribution += 5;

                triggeredChecks.Add($"Transactions (Retail) count last 30 days: {context.ClientActivity.RetailTransactionCountLast30Days} [max range: {retailTransactionCountLast30DaysMinRange}]");
            }


            // #6
            if(context.ClientActivity.IncomingFromGamblingMerchantsCountLast24h >= incomingFromGamblingMerchantsCountLast24hMinRange)
            {
                matchedCount++;
                contribution += 15;

                triggeredChecks.Add($"Payment from gambling merchant count last 24h: {context.ClientActivity.IncomingFromGamblingMerchantsCountLast24h} [min range: {incomingFromGamblingMerchantsCountLast24hMinRange}]");
            }


            bool isTriggered = contribution >= typologyConfidenceScoreThreshold;

            if (triggeredChecks.Count > 0)
            {
                details = $"Triggered checks:\n- {string.Join("\n- ", triggeredChecks)}";

            }
            else
            {
                details = "Triggered checks: none";
            }


            return new RuleResult(isTriggered, matchedCount, GamblingThresholds.TotalChecks, contribution, details);

        }


    }
}

