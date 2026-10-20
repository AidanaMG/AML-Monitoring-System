namespace Aml.Simulator.ProfilesAdditions
{
    public static class GeneratorThresholds
    {
        public static class Amount
        {
            public const int AccountBalanceMaxRange = 5_000_000;
            public const int AccountBalanceMinRange = 50;

            public const int SmallAmountMinRange = 3;
            public const int SmallAmountMaxMultiplier = 21;
            public const int LargeAmountMinRange = 100;
            public const int LargeAmountMaxMultiplier = 400;
            public const int LargeAmountPayoutMaxMultiplier = 1_000;

            public const int CryptoUnevenBaseMinRange = 10;
            public const int CryptoUnevenBaseMaxRange = 1_000;

            public const int CryptoTransferMinRange = 100;
            public const int CryptoTransferMaxRange = 100_000;
            public const int CryptoRetailMinRange = 1_000;
            public const int CryptoRetailMaxRange = 100_000;


        }


        public static class Datetime
        {
            public const int ClientJoinDateMinMonths = 1;
            public const int ClientJoinDateMaxMonths = 60;
            public const int FreshClientJoinDateMinMonths = 1;
            public const int FreshClientJoinDateMaxMonths = 3;

            public const int BackgroundTransactionsStartDateMonths = 6;

            public const int DropperTransactionsLast24hWindow = 24;

            public const int GamblingTransactionsStartWindowAfterJoinDate = 1;
            public const int GamblingTransactionsEndWindowAfterJoinDate = 3;
            public const int DoublingAmountMinMinutes = 1;
            public const int GamblingStartHour = 10;

            public const int CryptoStartWindow = 30;

            public const int DrugStartWindow24h = 24;

            public const int HighFrequencyMinutesMultiplier = 3;

        }


        public static class Frequency
        {
            public const int SalaryCount = 3;
            public const int BackgroundTransactionsCount = 15;

            public const int DropperP2pTransactionsCount = 13;
            public const int DropperOtherTransactionsCount = 8;

            public const int GamblingPayoutTransactionsCount = 5;

            public const int CryptoTransferTransactionsCount = 30;
            public const int CryptoRetailTransactionsCount = 10;

            public const int CountBuffer = 5;

        }

    }
}
