using Aml.Application;
using Aml.Application.RulesAbstractions;
using Aml.Domain.Entities;
using Aml.Domain.Enums;

namespace Aml.Infrastructure
{
    public static class RuleContextCalculator
    {
        static bool IsIncoming(Transaction t, HashSet<int> accountIds)
            => t.ToAccountId is int id && accountIds.Contains(id);

        static bool IsOutcoming(Transaction t, HashSet<int> accountIds)
            => t.FromAccountId is int id && accountIds.Contains(id);

        static bool IsInternal(Transaction t, HashSet<int> accountIds) 
            => IsIncoming(t, accountIds) && IsOutcoming(t, accountIds);


        public static AccountActivity BuildAccountActivity(Account account, List<Transaction> transactions30d,
            DateTime last24h, HashSet<int> accountIds)
        {
            var incomingP2p24h = transactions30d
                .Where(t => t.Type == TransactionType.P2p
                    && !IsInternal(t, accountIds)
                    && t.ToAccountId == account.Id
                    && t.TransactionDate > last24h)
                .ToList();


            return new AccountActivity
            {
                AccountId = account.Id,
                CurrentAccountBalance = account.Balance,
                IncomingP2pAmountSumLast24h = incomingP2p24h.Sum(t => t.Amount)
            };
        }


        public static ClientActivity BuildClientActivity(
            List<Transaction> transactions30d, 
            HashSet<int> accountIds,
            DateTime last1h,
            DateTime last24h,
            DateTime last3d,
            IReferenceDataCache cache) 
        {
            var retail30d = transactions30d
                .Where(t => t.Type == TransactionType.Retail)
                .ToList();

            var p2pAndIntrabank30d = transactions30d
                .Where(t => (t.Type == TransactionType.P2p || t.Type == TransactionType.IntrabankTransfer)
                    && !IsInternal(t, accountIds))
                .ToList();

            var transactionsLastHour = transactions30d
                .Where(t => t.TransactionDate > last1h)
                .ToList();


            var incomingP2p24h = transactions30d
                .Where(t => t.Type == TransactionType.P2p
                    && IsIncoming(t, accountIds) && !IsInternal(t, accountIds)
                    && t.TransactionDate > last24h)
                .ToList();

            var outcomingP2p24h = transactions30d
                .Where(t => t.Type == TransactionType.P2p
                    && IsOutcoming(t, accountIds) && !IsInternal(t, accountIds)
                    && t.TransactionDate > last24h)
                .ToList();

            var incomingP2p3d = transactions30d
                .Where(t => t.Type == TransactionType.P2p
                    && IsIncoming(t, accountIds) && !IsInternal(t, accountIds)
                    && t.TransactionDate > last3d)
                .ToList();

            var outcomingP2p3d = transactions30d
                .Where(t => t.Type == TransactionType.P2p
                    && IsOutcoming(t, accountIds) && !IsInternal(t, accountIds)
                    && t.TransactionDate > last3d)
                .ToList();


            Merchant? MerchantOf(Transaction t)
                => t.MerchantId is int id ? cache.GetMerchant(id) : null;

            var foreignRetail24h = retail30d
                .Where(t => t.TransactionDate > last24h)
                .Where(t => MerchantOf(t) is Merchant m
                    && m.CountryId is int cId && cId != cache.HomeCountryId)
                .ToList();

            var highRiskMerchantsLastHour = transactionsLastHour
                .Where(t => MerchantOf(t) is Merchant m
                    && cache.GetMccCode(m.MccId)?.RiskLevel == RiskLevel.High)
                .ToList();

            var nonAifc30d = retail30d
                .Where(t => MerchantOf(t)?.IsAifc == false)
                .ToList();

            var merchantCategories24h = retail30d
                .Where(t => t.TransactionDate > last24h)
                .Select(t => MerchantOf(t))
                .Where(m => m is not null)
                .Select(m => cache.GetMccCode(m!.MccId)?.Category)
                .Where(c => c is not null)
                .Distinct()
                .ToList(); 


            bool IsHighRiskCountry(int? countryId)
                => countryId is int id && cache.GetCountry(id)?.RiskLevel == RiskLevel.High;

            var highRiskCountryCount3d = transactions30d
                .Where(t => t.TransactionDate > last3d)
                .Count(t => IsHighRiskCountry(t.TransactionCountryId)
                         || IsHighRiskCountry(t.CounterpartyCountryId)
                         || IsHighRiskCountry(MerchantOf(t)?.CountryId));


            return new ClientActivity
            {
                RetailTransactionCountLast30Days = retail30d.Count(),
                P2pAndIntrabankTransactionCountLast30Days = p2pAndIntrabank30d.Count(),
                P2pAndIntrabankAmountsLast30Days = p2pAndIntrabank30d.Select(t => t.Amount).ToList(),
                TransactionCountLastHour = transactionsLastHour.Count(),

                IncomingP2pAmountsLast24h = incomingP2p24h.Select(t => t.Amount).ToList(),
                OutcomingP2pAmountsLast24h = outcomingP2p24h.Select(t => t.Amount).ToList(),
                IncomingP2pTransactionsLast3Days = incomingP2p3d.Select(t => (t.Amount, t.TransactionDate)).ToList(),
                OutcomingP2pTransactionsLast3Days = outcomingP2p3d.Select(t => (t.Amount, t.TransactionDate)).ToList(),

                UniqueIncomingCounterpartiesLast24h = incomingP2p24h
                    .Select(t => t.CounterpartyName)
                    .Where(n => n is not null)
                    .Distinct().Count(),
                UniqueOutcomingCounterpartiesLast24h = outcomingP2p24h
                    .Select(t => t.CounterpartyName)
                    .Where(n => n is not null)
                    .Distinct().Count(),
                UniqueIncomingCardCountLast24h = incomingP2p24h
                    .Select(t => t.FromCardNumber)
                    .Where(n => n is not null)
                    .Distinct().Count(),
                UniqueCounterpartyCountLast30Days = p2pAndIntrabank30d
                    .Select(t => t.CounterpartyName)
                    .Where(n => n is not null)
                    .Distinct().Count(),
                NonResidentCounterpartyCountLast30Days = p2pAndIntrabank30d
                    .Where(t => t.CounterpartyCountryId is int id && id != cache.HomeCountryId)
                    .Select(t => t.CounterpartyName)
                    .Where(n => n is not null)
                    .Distinct().Count(),

                UniqueForeignMerchantTransactionCountLast24h = foreignRetail24h
                    .Select(t => t.MerchantId!.Value).Distinct().Count(),
                ForeignMerchantTransactionsLast24h = foreignRetail24h
                    .Select(t => (t.Amount, t.TransactionDate)).ToList(),
                HighRiskMerchantCountLastHour = highRiskMerchantsLastHour.Count(),
                NonAifcCryptoExchangeTransactionCountLast30Days = nonAifc30d.Count(),
                MerchantCategoryCountLast24h = merchantCategories24h.Count(),

                IncomingFromGamblingMerchantsCountLast24h = transactions30d
                    .Count(t => t.TransactionDate > last24h
                        && IsIncoming(t, accountIds)
                        && cache.IsKnownCounterparty(CounterpartyType.Gambling, t.CounterpartyName)),
                CounterpartiesInAntifraudListCount = incomingP2p24h.Concat(outcomingP2p24h)
                    .Select(t => t.CounterpartyName)
                    .Where(n => n is not null)
                    .Distinct()
                    .Count(n => cache.IsKnownCounterparty(CounterpartyType.AntifraudListed, n)),

                MinutesToFirstWithdrawalAfterIncoming = ComputeMinutesToFirstWithdrawal(incomingP2p24h, transactions30d, accountIds),

                TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days = highRiskCountryCount3d


            };
        }

        private static int? ComputeMinutesToFirstWithdrawal(List<Transaction> incomingP2p24h, 
                                                    List<Transaction> transactions30d,
                                                    HashSet<int> accountIds)
        {
            if(incomingP2p24h.Count == 0) return null;

            int? minutes = null;

            var outcomings = transactions30d
                .Where(t => IsOutcoming(t, accountIds) && !IsInternal(t, accountIds)
                    && (t.Type == TransactionType.P2p || t.Type == TransactionType.IntrabankTransfer
                     || t.Type == TransactionType.Atm || t.Type == TransactionType.CrossborderWireTransfer))
                .OrderBy(t => t.TransactionDate)
                .ToList();

            foreach (Transaction incoming in incomingP2p24h)
            {
                var firstOut = outcomings.FirstOrDefault(t => t.TransactionDate > incoming.TransactionDate);

                if(firstOut is not null)
                {
                    var diff = (int)(firstOut.TransactionDate - incoming.TransactionDate).TotalMinutes;
                    minutes = minutes is null ? diff : Math.Min(minutes.Value, diff); 
                }
            }

            return minutes;
        }
    }
}
