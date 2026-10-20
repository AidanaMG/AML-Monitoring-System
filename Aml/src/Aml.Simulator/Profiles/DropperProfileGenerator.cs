using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;


namespace Aml.Simulator.Profiles
{
    public class DropperProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "DROPPER_PROFILE";

        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();


        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.FreshClientJoinDateMinMonths, GeneratorThresholds.Datetime.FreshClientJoinDateMaxMonths)); 
            var dob = _utilities.GenerateDateOfBirth(random, 18, 25);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Low, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMinRange), 2); 
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);


            var transactions = new List<Transaction>();
            var fanInTransactions = new List<Transaction>();
            var withdrawalTransactions = new List<Transaction>();

            var weightedTypes = new List<TransactionType>();
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.Atm, 45));
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.IntrabankTransfer, 50));
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.CrossborderWireTransfer, 5));

            var endWindow = DateTime.Now;
            var startWindow = endWindow.AddHours(-GeneratorThresholds.Datetime.DropperTransactionsLast24hWindow)
                .AddMinutes(DropperThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange);

            var dropperCounterparties = new List<string>();
            dropperCounterparties.AddRange(reference.AntifraudCounterpartyNames.OrderBy(_ => random.Next()).Take(DropperThresholds.CounterpartiesInAntifraudListMinRange));

            var uniqueCounterparties = new HashSet<string>();
            while(uniqueCounterparties.Count < DropperThresholds.UniqueCounterpartiesLast24hMinRange)
            {
                uniqueCounterparties.Add(_utilities.GenerateCounterpartyName(random));
            }

            dropperCounterparties.AddRange(uniqueCounterparties);

            var weightedStatuses = _utilities.GetStandardWeightedStatuses();

            // FAN IN: P2P
            for(int i = 0; i < GeneratorThresholds.Frequency.DropperP2pTransactionsCount; i++)
            {
                var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(TransactionType.P2p, true, account, random);
                decimal amount = DropperThresholds.P2pAmountsLast24hMinRange + random.Next(0, 5) * 10_000;
                var currentTxDate = _utilities.GenerateTransactionsDateTime(random, 10, 12, startWindow, endWindow);

                var counterparty = i < dropperCounterparties.Count
                    ? dropperCounterparties[i]
                    : dropperCounterparties[random.Next(dropperCounterparties.Count)];


                fanInTransactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.P2p,
                    Direction = TransactionDirection.Credit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],
                    Amount =  amount, 
                    AmountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   
                    FromAccountId = transactionDetailsBasedOnType.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnType.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnType.FromCard,
                    ToAccountId = transactionDetailsBasedOnType.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnType.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnType.ToCard,
                    CounterpartyBank = transactionDetailsBasedOnType.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = null, 
                    CounterpartyName = counterparty,

                    IsFlagged = false

                });
            }

            // FAN IN: OTHER
            for (int i = 0; i < GeneratorThresholds.Frequency.DropperOtherTransactionsCount; i++)
            {
                var randomTxType = weightedTypes[random.Next(weightedTypes.Count)];
                var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(randomTxType, true, account, random);
                decimal amount = DropperThresholds.P2pAmountsLast24hMinRange + random.Next(0, 60) * 10_000; 
                var currentTxDate = _utilities.GenerateTransactionsDateTime(random, 8, 11, startWindow, endWindow);

                fanInTransactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = randomTxType,
                    Direction = TransactionDirection.Credit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],
                    Amount = amount,
                    AmountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   
                    FromAccountId = transactionDetailsBasedOnType.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnType.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnType.FromCard,
                    ToAccountId = transactionDetailsBasedOnType.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnType.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnType.ToCard,
                    CounterpartyBank = transactionDetailsBasedOnType.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = _utilities.IsRetail(randomTxType) ? $"{reference.GeneralMerchants[random.Next(reference.GeneralMerchants.Count)].Name}" : null, 
                    CounterpartyName = randomTxType == TransactionType.P2p || randomTxType == TransactionType.IntrabankTransfer || randomTxType == TransactionType.CrossborderWireTransfer ? dropperCounterparties[random.Next(dropperCounterparties.Count)] : null,

                    IsFlagged = false

                });

            }


            // WITHDRAWAL: P2P
            fanInTransactions = fanInTransactions.OrderBy(t => t.TransactionDate).ToList();
            var lastFanInDate = fanInTransactions
                .Where(t => t.Type == TransactionType.P2p)
                .Max(t => t.TransactionDate);
            
            for(int j = 0; j < GeneratorThresholds.Frequency.DropperP2pTransactionsCount; j++)
            {
                var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(TransactionType.P2p, false, account, random);
                decimal amount = DropperThresholds.P2pAmountsLast24hMinRange + random.Next(0, 5) * 10_000;
                var currentTxDate = lastFanInDate.AddMinutes(random.Next(2, DropperThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange));

                var counterparty = j < dropperCounterparties.Count
                    ? dropperCounterparties[j]
                    : dropperCounterparties[random.Next(dropperCounterparties.Count)];

                withdrawalTransactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.P2p,
                    Direction = TransactionDirection.Debit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],
                    Amount = amount,
                    AmountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   
                    FromAccountId = transactionDetailsBasedOnType.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnType.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnType.FromCard,
                    ToAccountId = transactionDetailsBasedOnType.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnType.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnType.ToCard,
                    CounterpartyBank = transactionDetailsBasedOnType.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = null, 
                    CounterpartyName = counterparty,

                    IsFlagged = false

                });
            }


            // WITHDRAWAL: OTHER
            for(int j = 0; j < GeneratorThresholds.Frequency.DropperOtherTransactionsCount; j++)
            {
                var randomTxType = weightedTypes[random.Next(weightedTypes.Count)];
                var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(randomTxType, false, account, random);
                decimal amount = DropperThresholds.P2pAmountsLast24hMinRange + random.Next(0, 60) * 10_000; 
                var currentTxDate = lastFanInDate.AddMinutes(random.Next(2, DropperThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange));

                withdrawalTransactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = randomTxType,
                    Direction = TransactionDirection.Debit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],

                    Amount = amount,
                    AmountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt),

                    Currency = account.Currency,
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   

                    FromAccountId = transactionDetailsBasedOnType.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnType.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnType.FromCard,

                    ToAccountId = transactionDetailsBasedOnType.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnType.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnType.ToCard,

                    CounterpartyBank = transactionDetailsBasedOnType.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = _utilities.IsRetail(randomTxType) ? $"{reference.GeneralMerchants[random.Next(reference.GeneralMerchants.Count)].Name}" : null, 

                    CounterpartyName = randomTxType == TransactionType.P2p || randomTxType == TransactionType.IntrabankTransfer || randomTxType == TransactionType.CrossborderWireTransfer ? dropperCounterparties[random.Next(dropperCounterparties.Count)] : null,

                    IsFlagged = false

                });
            }

            transactions.AddRange(fanInTransactions);
            transactions.AddRange(withdrawalTransactions);
            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList>());
        }

    }
}