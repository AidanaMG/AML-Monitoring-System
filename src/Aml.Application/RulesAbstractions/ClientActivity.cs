
namespace Aml.Application.RulesAbstractions
{
    public class ClientActivity
    {

        public int RetailTransactionCountLast30Days { get; init; }

        public int? MinutesToFirstWithdrawalAfterIncoming { get; init; }
        public IReadOnlyList<decimal> IncomingP2pAmountsLast24h { get; init; } = new List<decimal>();
        public IReadOnlyList<decimal> OutcomingP2pAmountsLast24h { get; init; } = new List<decimal>();

        public int UniqueIncomingCounterpartiesLast24h { get; init; }
        public int UniqueOutcomingCounterpartiesLast24h { get; init; }
        public int CounterpartiesInAntifraudListCount { get; init; } 

        public int UniqueForeignMerchantTransactionCountLast24h { get; init; }
        public IReadOnlyList<(decimal Amount, DateTime Time)> ForeignMerchantTransactionsLast24h { get; init; } = new List<(decimal, DateTime)>();
        public int MerchantCategoryCountLast24h { get; init; }
        public int IncomingFromGamblingMerchantsCountLast24h { get; init; }


        public IReadOnlyList<decimal> P2pAndIntrabankAmountsLast30Days { get; init; } = new List<decimal>(); 
        public int P2pAndIntrabankTransactionCountLast30Days { get; init; } 
        public int UniqueCounterpartyCountLast30Days { get; init; } 
        public int NonResidentCounterpartyCountLast30Days { get; init; } 
        public int NonAifcCryptoExchangeTransactionCountLast30Days { get; init; }


        public IReadOnlyList<(decimal Amount, DateTime Time)> IncomingP2pTransactionsLast3Days { get; init; } = new List<(decimal, DateTime)>();
        public IReadOnlyList<(decimal Amount, DateTime Time)> OutcomingP2pTransactionsLast3Days { get; init; } = new List<(decimal, DateTime)>();
        public int UniqueIncomingCardCountLast24h { get; init; }

        public int TransactionCountLastHour { get; init; }
        public int HighRiskMerchantCountLastHour { get; init; }

        public int TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days { get; init; }
    }
}
