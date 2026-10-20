using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;


namespace Aml.Simulator.Profiles
{
    public class SimpleRiskLevelProfileGenerator : IProfileGenerator
    {
        private readonly RiskScenario _scenario;

        public SimpleRiskLevelProfileGenerator(RiskScenario scenario)
        {
            _scenario = scenario;
        }

        public string ProfileName => $"{_scenario}_PROFILE";


        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();

        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.ClientJoinDateMinMonths, GeneratorThresholds.Datetime.ClientJoinDateMaxMonths));
            var dob = _utilities.GenerateDateOfBirth(random, 18, 70);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Low, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMaxRange), 2);
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);

            var startDate = DateTime.Now.AddMonths(-GeneratorThresholds.Datetime.BackgroundTransactionsStartDateMonths);
            var transactions = _baseClient.CreateBackgroundTransactions(random, account, startDate, DateTime.Now, 1, reference);


            var weightedTypes = new List<TransactionType>();
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.Retail, 70));
            int p2pWeight = 7;
            if (account.Type == AccountType.Card)
            {
                weightedTypes.AddRange(Enumerable.Repeat(TransactionType.P2p, p2pWeight));
                p2pWeight = 0;
            }
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.Atm, 3));
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.IntrabankTransfer, 17 + p2pWeight));
            weightedTypes.AddRange(Enumerable.Repeat(TransactionType.CrossborderWireTransfer, 3));

            var weightedStatuses = _utilities.GetStandardWeightedStatuses();

            var txDate = DateTime.Now;
            var randomTxType = weightedTypes[random.Next(weightedTypes.Count)];
            var randomTxStatus = weightedStatuses[random.Next(weightedStatuses.Count)];
            var randomDirection = _utilities.GetRandomTransactionDirection(randomTxType, random);

            var isCredit = randomDirection == TransactionDirection.Credit;

            var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(randomTxType, isCredit, account, random);
            var merchantTransactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(TransactionType.Retail, true, account, random);


            var criticalMerchant = reference.CriticalRiskMerchants
                .First(m => m.CountryId == reference.HomeCountryId && m.IsAifc ==  null);
            var highMerchant = reference.HighRiskMerchants
                .First(m => m.CountryId == reference.HomeCountryId && m.IsAifc == null);

            var merchantRange = _utilities.GetAmountRange(TransactionType.Retail, account.Currency);
            decimal merchantAmount = Math.Round(merchantRange.Min + (decimal)random.NextDouble() * (merchantRange.Max - merchantRange.Min), 2);
            decimal merchantAmountKzt = _utilities.ConvertToKzt(merchantAmount, account.Currency, reference.RatesToKzt);

            var range = _utilities.GetAmountRange(randomTxType, account.Currency);
            decimal amount = Math.Round(range.Min + (decimal)random.NextDouble() * (range.Max - range.Min), 2);
            decimal amountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt);


            switch(_scenario)
            {
                case RiskScenario.CriticalCountry:
                    transactions.Add(new Transaction
                    {
                        Id = 0, 
                        Type = randomTxType,
                        Direction = randomDirection,
                        Status = randomTxStatus,
                        Amount = amount, 
                        AmountKzt = amountKzt,
                        Currency = account.Currency,
                        TransactionDate = txDate,
                        
                        TransactionCountryId = reference.CriticalRiskCountryIds[random.Next(reference.CriticalRiskCountryIds.Length)],   

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
                        CounterpartyName = randomTxType == TransactionType.P2p || randomTxType == TransactionType.IntrabankTransfer || randomTxType == TransactionType.CrossborderWireTransfer ? _utilities.GenerateCounterpartyName(random) : null,

                        IsFlagged = false
                    });
                    break;

                case RiskScenario.HighCountry:
                    for(int i = 0; i < CriticalAndHighRiskThresholds.HighRiskCountryTransactions3dMinRange; i++)
                    {
                        var highCountryDate = txDate.AddDays(-i);

                        transactions.Add(new Transaction
                        {
                            Id = 0, 
                            Type = randomTxType,
                            Direction = randomDirection,
                            Status = randomTxStatus,
                            Amount = amount,
                            AmountKzt = amountKzt,
                            Currency = account.Currency,
                            TransactionDate = highCountryDate,

                            TransactionCountryId = reference.HighRiskCountryIds[random.Next(reference.HighRiskCountryIds.Length)],   

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
                            CounterpartyName = randomTxType == TransactionType.P2p || randomTxType == TransactionType.IntrabankTransfer || randomTxType == TransactionType.CrossborderWireTransfer ? _utilities.GenerateCounterpartyName(random) : null,

                            IsFlagged = false
                        });
                    }

                    break;

                case RiskScenario.CriticalMerchant:
                    transactions.Add(new Transaction
                    {
                        Id = 0, 
                        Type = TransactionType.Retail,
                        Direction = TransactionDirection.Debit,
                        Status = randomTxStatus,
                        Amount = merchantAmount,
                        AmountKzt = merchantAmountKzt,
                        Currency = account.Currency,
                        TransactionDate = txDate,
                        TransactionCountryId = null,   
                        FromAccountId = merchantTransactionDetailsBasedOnType.FromId,   
                        FromAccountNumber = merchantTransactionDetailsBasedOnType.FromAccount,
                        FromCardNumber = merchantTransactionDetailsBasedOnType.FromCard,
                        ToAccountId = merchantTransactionDetailsBasedOnType.ToId,    
                        ToAccountNumber = merchantTransactionDetailsBasedOnType.ToAccount,
                        ToCardNumber = merchantTransactionDetailsBasedOnType.ToCard,
                        CounterpartyBank = merchantTransactionDetailsBasedOnType.CounterpartyBank,
                        CounterpartyCountryId = null,  
                        MerchantId = criticalMerchant.Id,  
                        MerchantName = criticalMerchant.Name, 
                        CounterpartyName = null,

                        IsFlagged = false
                    });
                    break;


                case RiskScenario.HighMerchant:
                    for(int i = 0; i < CriticalAndHighRiskThresholds.HighRiskMerchantCountLastHourMinRange; i++)
                    {
                        var highMerchantDate = DateTime.Now.AddMinutes(-i * GeneratorThresholds.Datetime.HighFrequencyMinutesMultiplier);

                        transactions.Add(new Transaction
                        {
                            Id = 0, 
                            Type = TransactionType.Retail,
                            Direction = TransactionDirection.Debit,
                            Status = randomTxStatus,
                            Amount = merchantAmount,
                            AmountKzt = merchantAmountKzt,
                            Currency = account.Currency,
                            TransactionDate = highMerchantDate,
                            TransactionCountryId = null,   
                            FromAccountId = merchantTransactionDetailsBasedOnType.FromId,   
                            FromAccountNumber = merchantTransactionDetailsBasedOnType.FromAccount,
                            FromCardNumber = merchantTransactionDetailsBasedOnType.FromCard,
                            ToAccountId = merchantTransactionDetailsBasedOnType.ToId,    
                            ToAccountNumber = merchantTransactionDetailsBasedOnType.ToAccount,
                            ToCardNumber = merchantTransactionDetailsBasedOnType.ToCard,
                            CounterpartyBank = merchantTransactionDetailsBasedOnType.CounterpartyBank,
                            CounterpartyCountryId = null,  
                            MerchantId = highMerchant.Id,  
                            MerchantName = highMerchant.Name, 
                            CounterpartyName = null,

                            IsFlagged = false
                        });
                    }
                    break;


                case RiskScenario.HighFrequency:
                    for(int i = 0; i <= CriticalAndHighRiskThresholds.TransactionCountLastHourMinRange + GeneratorThresholds.Frequency.CountBuffer; i++)    
                    {
                        var highFrequencyDate = DateTime.Now.AddMinutes(-i * GeneratorThresholds.Datetime.HighFrequencyMinutesMultiplier);
                       
                        transactions.Add(new Transaction
                        {
                            Id = 0, 
                            Type = randomTxType,
                            Direction = randomDirection,
                            Status = randomTxStatus,

                            Amount = amount,
                            AmountKzt = amountKzt,

                            Currency = account.Currency,

                            TransactionDate = highFrequencyDate,

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
                            CounterpartyName = randomTxType == TransactionType.P2p || randomTxType == TransactionType.IntrabankTransfer || randomTxType == TransactionType.CrossborderWireTransfer ? _utilities.GenerateCounterpartyName(random) : null,

                            IsFlagged = false
                        });
                    }

                    
                    break;


                case RiskScenario.CombinedScoring:
                    for(int i = 0; i < CriticalAndHighRiskThresholds.TransactionCountLastHourMinRange + GeneratorThresholds.Frequency.CountBuffer; i++)
                    {
                        var date = DateTime.Now.AddMinutes(-i * GeneratorThresholds.Datetime.HighFrequencyMinutesMultiplier);

                        transactions.Add(new Transaction
                        {
                            Id = 0, 
                            Type = TransactionType.Retail,
                            Direction = TransactionDirection.Debit,
                            Status = randomTxStatus,
                            Amount = merchantAmount,
                            AmountKzt = merchantAmountKzt,
                            Currency = account.Currency,
                            TransactionDate = date,
                            TransactionCountryId = null,
                            FromAccountId = merchantTransactionDetailsBasedOnType.FromId,   
                            FromAccountNumber = merchantTransactionDetailsBasedOnType.FromAccount,
                            FromCardNumber = merchantTransactionDetailsBasedOnType.FromCard,
                            ToAccountId = merchantTransactionDetailsBasedOnType.ToId,    
                            ToAccountNumber = merchantTransactionDetailsBasedOnType.ToAccount,
                            ToCardNumber = merchantTransactionDetailsBasedOnType.ToCard,
                            CounterpartyBank = merchantTransactionDetailsBasedOnType.CounterpartyBank,
                            CounterpartyCountryId = null,  
                            MerchantId = highMerchant.Id,  
                            MerchantName = highMerchant.Name, 
                            CounterpartyName = null,

                            IsFlagged = false
                        });
                    }
                    break;
            }


            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList>());

        }
        

    }
}