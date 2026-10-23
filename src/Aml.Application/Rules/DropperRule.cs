using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    public class DropperRule : ITransactionRule
    {
        public string Code => "DROPPER";
        public RuleType Type => RuleType.Typology;


        private static bool IsRoundAmount(decimal amount)
        {
            return amount % 10_000 == 0; 
        }


        public RuleResult Evaluate(RuleContext context)
        {
            List<string> triggeredChecks = new();

            int matchedCount = 0;
            int contribution = 0;
            string details = "";

            var typologyConfidenceScoreThreshold = context.Settings.Get(Code, "typologyConfidenceScoreThreshold",
                ScreeningThresholds.TypologyConfidenceScoreThreshold);
            var clientAgeDaysMinRange = context.Settings.Get(Code, "clientAgeDaysMinRange",
                DropperThresholds.ClientAgeDaysMinRange);
            var retailTransactionCountLast30DaysMinRange = context.Settings.Get(Code, "retailTransactionCountLast30DaysMinRange",
                DropperThresholds.RetailTransactionCountLast30DaysMinRange);
            var balancePercentageMinRange = context.Settings.Get(Code, "balancePercentageMinRange",
                DropperThresholds.BalancePercentageMinRange);
            var minutesToFirstWithdrawalAfterIncomingMaxRange = context.Settings.Get(Code, "minutesToFirstWithdrawalAfterIncomingMaxRange",
                DropperThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange);
            var p2pAmountsLast24hMinRange = context.Settings.Get(Code, "p2pAmountsLast24hMinRange",
                DropperThresholds.P2pAmountsLast24hMinRange);
            var p2pCountMinRange = context.Settings.Get(Code, "p2pCountMinRange",
                DropperThresholds.P2pCountMinRange);
            var uniqueCounterpartiesLast24hMinRange = context.Settings.Get(Code, "uniqueCounterpartiesLast24hMinRange",
                DropperThresholds.UniqueCounterpartiesLast24hMinRange);
            var counterpartiesInAntifraudListMinRange = context.Settings.Get(Code, "counterpartiesInAntifraudListMinRange",
                DropperThresholds.CounterpartiesInAntifraudListMinRange);


            // #1
            if (context.ClientFacts.ClientAgeDays < clientAgeDaysMinRange)  
            {
                matchedCount++;
                contribution += 5; 

                triggeredChecks.Add($"Client registered: {context.ClientFacts.ClientAgeDays} days ago [max range: {clientAgeDaysMinRange} days]");
            }

            // #2
            if(context.ClientActivity.RetailTransactionCountLast30Days < retailTransactionCountLast30DaysMinRange) 
            {
                matchedCount++;
                contribution += 5; 

                triggeredChecks.Add($"Transactions (Retail) count last 30 days: {context.ClientActivity.RetailTransactionCountLast30Days} [max range: {retailTransactionCountLast30DaysMinRange}]");
            }


            // #3
            var balanceDroppedAccounts = context.Accounts
                .Where(a => a.IncomingP2pAmountSumLast24h > 0
                    && a.CurrentAccountBalance < a.IncomingP2pAmountSumLast24h * balancePercentageMinRange)
                .ToList();

            if (balanceDroppedAccounts.Count > 0) 
            {
                matchedCount++; 
                contribution += 10; 

                triggeredChecks.Add($"Account id-s with balance drop last 24h: {string.Join(", ", balanceDroppedAccounts.Select(a => a.AccountId))} [min percentage of income P2P: {balancePercentageMinRange * 100}%]");
            }


            // #4
            if(context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming is not null && context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming < minutesToFirstWithdrawalAfterIncomingMaxRange) 
            {
                matchedCount++;
                contribution += 15; 

                triggeredChecks.Add($"Time (minutes) to withdrawal after income: {context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming} [max range: {minutesToFirstWithdrawalAfterIncomingMaxRange}]");
            }




            // #5 + #6  
            bool hasIncomingEvenAmountsPattern = false;
            bool hasOutcomingEvenAmountsPattern = false;


            // INCOMING
            if(context.ClientActivity.IncomingP2pAmountsLast24h.Count >= p2pCountMinRange)
            {
                var mostIncomingFrequentAmount = context.ClientActivity.IncomingP2pAmountsLast24h
                    .GroupBy(a => a)
                    .OrderByDescending(e => e.Count())
                    .First().Key; 

                if(mostIncomingFrequentAmount >= p2pAmountsLast24hMinRange && IsRoundAmount(mostIncomingFrequentAmount))
                {
                    hasIncomingEvenAmountsPattern = true;
                }

            }

            if(hasIncomingEvenAmountsPattern)
            {
                matchedCount++;
                contribution += 20; 

                triggeredChecks.Add($"Transactions (P2P - income) with even amounts pattern last 24h is detected | Transactions (P2P - income) count: {context.ClientActivity.IncomingP2pAmountsLast24h.Count} [min range: {p2pCountMinRange}]");
            }

            // OUTCOMING 
            if(context.ClientActivity.OutcomingP2pAmountsLast24h.Count >= p2pCountMinRange)
            {
                var mostOutcomingFrequentAmount = context.ClientActivity.OutcomingP2pAmountsLast24h
                    .GroupBy(a => a)
                    .OrderByDescending(e => e.Count())
                    .First().Key;

                if (mostOutcomingFrequentAmount >= p2pAmountsLast24hMinRange && IsRoundAmount(mostOutcomingFrequentAmount))
                {
                    hasOutcomingEvenAmountsPattern = true;
                }
            }

            if(hasOutcomingEvenAmountsPattern)
            {
                matchedCount++;
                contribution += 20; 

                triggeredChecks.Add($"Transactions (P2P - outcome) with even amounts pattern last 24h is detected | Transactions (P2P - outcome) count: {context.ClientActivity.OutcomingP2pAmountsLast24h.Count} [min range: {p2pCountMinRange}]");
            }



            // #7
            if (context.ClientActivity.UniqueIncomingCounterpartiesLast24h >= uniqueCounterpartiesLast24hMinRange || context.ClientActivity.UniqueOutcomingCounterpartiesLast24h >= uniqueCounterpartiesLast24hMinRange) 
            {
                matchedCount++;
                contribution += 12; 

                triggeredChecks.Add($"Unique counterparties (income) count last 24h: {context.ClientActivity.UniqueIncomingCounterpartiesLast24h} | Unique counterparties (outcome) count last 24h: {context.ClientActivity.UniqueOutcomingCounterpartiesLast24h} [min range: {uniqueCounterpartiesLast24hMinRange}]"); 
            }

            // #8
            if(context.ClientActivity.CounterpartiesInAntifraudListCount >= counterpartiesInAntifraudListMinRange) 
            {
                matchedCount++;
                contribution += 13;

                triggeredChecks.Add($"Counterparty in antifraud list count: {context.ClientActivity.CounterpartiesInAntifraudListCount} [min range: {counterpartiesInAntifraudListMinRange}]"); 
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

            return new RuleResult(isTriggered, matchedCount, DropperThresholds.TotalChecks, contribution, details);

        }
    }
}

