using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;
using Aml.Domain.Enums;

namespace Aml.Application.Rules
{
    /*
       TODO: regional risk within Kazakhstan (different oblasts/cities) is not modeled — 
       Client/Transaction only carry a free-text Address string, with no structured 
       City/Region field in Domain. Parsing region out of free text would be unreliable. 
       Cross-border high-risk-jurisdiction geography (narco-source countries) is covered 
       instead via existing TransactionCountryRisk/CounterpartyCountryRisk.
     */

    public class DrugRule : ITransactionRule
    {
        public string Code => "DRUG";
        public RuleType Type => RuleType.Typology;


        public RuleResult Evaluate(RuleContext context)
        {
            List<string> triggeredChecks = new();
            int matchedCount = 0;
            int contribution = 0;
            string details = "";

            var typologyConfidenceScoreThreshold = context.Settings.Get(Code, "typologyConfidenceScoreThreshold",
                ScreeningThresholds.TypologyConfidenceScoreThreshold);
            var clientAgeDaysMinRange = context.Settings.Get(Code, "clientAgeDaysMinRange",
                DrugThresholds.ClientAgeDaysMinRange);
            var clientAgeMinRange = context.Settings.Get(Code, "clientAgeMinRange",
                DrugThresholds.ClientAgeMinRange);
            var clientAgeMaxRange = context.Settings.Get(Code, "clientAgeMaxRange",
                DrugThresholds.ClientAgeMaxRange);
            var uniqueIncomingCounterpartiesLast24hMinRange = context.Settings.Get(Code, "uniqueIncomingCounterpartiesLast24hMinRange",
                DrugThresholds.UniqueIncomingCounterpartiesLast24hMinRange);
            var uniqueIncomingCardCountLast24hMinRange = context.Settings.Get(Code, "uniqueIncomingCardCountLast24hMinRange",
                DrugThresholds.UniqueIncomingCardCountLast24hMinRange);
            var incomingP2pCountLast24hMinRange = context.Settings.Get(Code, "incomingP2pCountLast24hMinRange",
                DrugThresholds.IncomingP2pCountLast24hMinRange);
            var drugPriceMinRange = context.Settings.Get(Code, "drugPriceMinRange",
                DrugThresholds.DrugPriceMinRange);
            var drugPriceMaxRange = context.Settings.Get(Code, "drugPriceMaxRange",
                DrugThresholds.DrugPriceMaxRange);
            var balancePercentageMinRange = context.Settings.Get(Code, "balancePercentageMinRange",
                DrugThresholds.BalancePercentageMinRange);
            var minutesToFirstWithdrawalAfterIncomingMaxRange = context.Settings.Get(Code, "minutesToFirstWithdrawalAfterIncomingMaxRange",
                DrugThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange);
            var incomingAndOutcomingP2pCount3dMinRange = context.Settings.Get(Code, "incomingAndOutcomingP2pCount3dMinRange",
                DrugThresholds.IncomingAndOutcomingP2pCount3dMinRange);


            // #1 
            if (context.ClientFacts.ClientAgeDays < clientAgeDaysMinRange)
            {
                matchedCount++;
                contribution += 5;

                triggeredChecks.Add($"Client registered: {context.ClientFacts.ClientAgeDays} days ago [max range: {clientAgeDaysMinRange} days]");

            }


            // #2 
            if(context.ClientFacts.ClientAge >= clientAgeMinRange && context.ClientFacts.ClientAge <= clientAgeMaxRange)
            {
                matchedCount++;
                contribution += 5;

                triggeredChecks.Add($"Client age: {context.ClientFacts.ClientAge} [min range: {clientAgeMinRange} | max range: {clientAgeMaxRange}]");

            }


            // #3 
            if(context.ClientActivity.IncomingP2pAmountsLast24h.Count >= incomingP2pCountLast24hMinRange) 
            {
                var mostFrequentAmount = context.ClientActivity.IncomingP2pAmountsLast24h
                    .GroupBy(a => a)
                    .OrderByDescending(o => o.Count())
                    .First().Key;

                if(mostFrequentAmount >= drugPriceMinRange && mostFrequentAmount <= drugPriceMaxRange)
                {
                    matchedCount++;
                    contribution += 20;

                    triggeredChecks.Add($"Most frequent transaction amount last 24h: {mostFrequentAmount} [min range: {drugPriceMinRange} | max range: {drugPriceMaxRange}]");
                    triggeredChecks.Add($"Transactions (P2P - income) count last 24h: { context.ClientActivity.IncomingP2pAmountsLast24h.Count} [min range: { incomingP2pCountLast24hMinRange}]");
                        
                }

            }


            // #4
            if(context.ClientActivity.UniqueIncomingCardCountLast24h >= uniqueIncomingCardCountLast24hMinRange
                && context.ClientActivity.UniqueIncomingCounterpartiesLast24h >= uniqueIncomingCounterpartiesLast24hMinRange)
            {
                matchedCount++;
                contribution += 20;

                triggeredChecks.Add($"Unique card (income) count last 24h: {context.ClientActivity.UniqueIncomingCardCountLast24h} [min range: {uniqueIncomingCardCountLast24hMinRange}]");
                triggeredChecks.Add($"Unique counterparty (income) count last 24h: {context.ClientActivity.UniqueIncomingCounterpartiesLast24h} [min range: {uniqueIncomingCounterpartiesLast24hMinRange}]");



            }


            // #5 
            var p2pLast3d = context.ClientActivity.IncomingP2pTransactionsLast3Days.Select(t => t.Time.Date)
                .Concat(context.ClientActivity.OutcomingP2pTransactionsLast3Days.Select(t => t.Time.Date))
                .Distinct()
                .OrderBy(d => d).ToList();

            var hasTwoConsecutiveDays = false;
            for(int i = 0; i < p2pLast3d.Count - 1; i++)
            {
                if((p2pLast3d[i+1] - p2pLast3d[i]).Days == 1)
                {
                    hasTwoConsecutiveDays = true;
                    break;
                }
            }

            var totalP2pCount3d = context.ClientActivity.IncomingP2pTransactionsLast3Days.Count() + context.ClientActivity.OutcomingP2pTransactionsLast3Days.Count;

            if(hasTwoConsecutiveDays && totalP2pCount3d >= incomingAndOutcomingP2pCount3dMinRange)
            {
                matchedCount++;
                contribution += 15;

                triggeredChecks.Add($"2 consecutive days of P2P activity is detected | Transactions (P2P) total count last 3 days: {totalP2pCount3d} [min range: {incomingAndOutcomingP2pCount3dMinRange}]");

            }


            // #6 
            var balanceDroppedAccounts = context.Accounts
                .Where(a => a.IncomingP2pAmountSumLast24h > 0
                    && a.CurrentAccountBalance < a.IncomingP2pAmountSumLast24h * balancePercentageMinRange)
                .ToList();

            if(balanceDroppedAccounts.Count > 0)
            {
                matchedCount++;
                contribution += 20;

                triggeredChecks.Add($"Account id-s with balance drop last 24h: {string.Join(", ", balanceDroppedAccounts.Select(a => a.AccountId))} [min percentage of income P2P: {balancePercentageMinRange * 100}%]");

            }


            // #7
            if(context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming is not null && context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming < minutesToFirstWithdrawalAfterIncomingMaxRange)
            {
                matchedCount++;
                contribution += 15;

                triggeredChecks.Add($"Time (minutes) to withdrawal after income: {context.ClientActivity.MinutesToFirstWithdrawalAfterIncoming} [max range: {minutesToFirstWithdrawalAfterIncomingMaxRange}]");
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


            return new RuleResult(isTriggered, matchedCount, DrugThresholds.TotalChecks, contribution, details);

        }

    }
}
