
namespace Aml.Application.Screening
{

    public static class ScreeningThresholds
    {
        public const int ScoringScoreThreshold = 70;
        public const int TypologyConfidenceScoreThreshold = 70;
        
    }


    public static class DropperThresholds
    {
        public const int TotalChecks = 8;
        public const int ClientAgeDaysMinRange = 90;
        public const int RetailTransactionCountLast30DaysMinRange = 20;
        public const decimal BalancePercentageMinRange = 0.2m;
        public const int MinutesToFirstWithdrawalAfterIncomingMaxRange = 15;
        public const decimal P2pAmountsLast24hMinRange = 100_000m;
        public const int P2pCountMinRange = 10;
        public const int UniqueCounterpartiesLast24hMinRange = 5;
        public const int CounterpartiesInAntifraudListMinRange = 1;
    }

    public static class GamblingThresholds
    {
        public const int TotalChecks = 6;
        public const int MerchantCategoryCountLast24hMinRange = 3;
        public const int IncomingFromGamblingMerchantsCountLast24hMinRange = 1;
        public const int RetailTransactionCountLast30DaysMinRange = 23;
        public const decimal DoubledAmountsTolerance = 5m;
        public const int UniqueForeignMerchantTransactionCountLast24hMinRange = 6;
        public const int MinutesDifferenceBetweenTransactionsMaxRange = 60;
        public const int FractionalAmountCountMinRange = 3;
    }

    public static class CryptoArbitrageThresholds
    {
        public const int TotalChecks = 5;
        public const decimal UnevenAmountRatioMinRange = 0.7m;
        public const int TransactionsForRatioCheckMinRange = 5;
        public const int P2pAndIntrabankCountLast30DaysMinRange = 25;
        public const int UniqueCounterpartyCountLast30DaysMinRange = 25;
        public const int NonResidentCounterpartyCountLast30DaysMinRange = 5;
        public const int NonAifcCryptoExchangeTransactionCountLast30DaysMinRange = 1;
    }


    public static class DrugThresholds
    {
        public const int TotalChecks = 7;
        public const int ClientAgeDaysMinRange = 90;
        public const int ClientAgeMinRange = 18;
        public const int ClientAgeMaxRange = 25;
        public const int UniqueIncomingCounterpartiesLast24hMinRange = 5;
        public const int UniqueIncomingCardCountLast24hMinRange = 5;
        public const int IncomingP2pCountLast24hMinRange = 10;
        public const decimal DrugPriceMinRange = 15_000m;
        public const decimal DrugPriceMaxRange = 50_000m;
        public const int DistinctDaysWithActivityMinRange = 2;
        public const decimal BalancePercentageMinRange = 0.2m;
        public const int MinutesToFirstWithdrawalAfterIncomingMaxRange = 15;
        public const int IncomingAndOutcomingP2pCount3dMinRange = 20;


    }

    public static class CriticalAndHighRiskThresholds
    {
        public const int CriticalRiskMerchantTotalChecks = 1;
        public const int HighRiskMerchantTotalChecks = 1;
        public const int HighRiskMerchantScoringScoreThreshold = 35;
        public const int HighRiskMerchantCountLastHourMinRange = 10;

        public const int CriticalRiskCountryTotalChecks = 3;
        public const int HighRiskCountryTotalChecks = 3;
        public const int HighRiskCountryTransactions3dMinRange = 3;

        public const int HighFrequencyTotalChecks = 1;
        public const int TransactionCountLastHourMinRange = 20;
        public const int HighFrequencyScoringScoreThreshold = 40;

    }

}
