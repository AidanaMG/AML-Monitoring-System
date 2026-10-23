using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;

namespace Aml.Simulator.Profiles
{
    public class CryptoArbitrageProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "CRYPTO_PROFILE";

        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();


        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.ClientJoinDateMinMonths, GeneratorThresholds.Datetime.ClientJoinDateMaxMonths));
            var dob = _utilities.GenerateDateOfBirth(random, 18, 50);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Low, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMaxRange), 2);
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);


            var endWindow = DateTime.Now;
            var startWindow = endWindow.AddDays(-GeneratorThresholds.Datetime.CryptoStartWindow); 

            var transactions = new List<Transaction>();



            
            List<(string CounterpartyName, int CountryId)> counterpartyPool = new();
            var usedNames = new HashSet<string>();

            while(counterpartyPool.Count(c => reference.NonHomeCountryIds.Contains(c.CountryId)) < CryptoArbitrageThresholds.NonResidentCounterpartyCountLast30DaysMinRange)
            {
                var name = _utilities.GenerateCounterpartyName(random);
                if(usedNames.Add(name))
                {
                    counterpartyPool.Add((name, reference.NonHomeCountryIds[random.Next(reference.NonHomeCountryIds.Length)]));
                }
            }

            while(counterpartyPool.Count < CryptoArbitrageThresholds.UniqueCounterpartyCountLast30DaysMinRange)
            {
                var name = _utilities.GenerateCounterpartyName(random);
                if(usedNames.Add(name))
                {
                    counterpartyPool.Add((name, reference.HomeCountryId));
                }
            }

            counterpartyPool = counterpartyPool.OrderBy(_ => random.Next()).ToList();



            var statuses = _utilities.GetStandardWeightedStatuses();


            // PHASE 1: TRANSFERS
            int unevenAmountsCounter = (int)Math.Ceiling(GeneratorThresholds.Frequency.CryptoTransferTransactionsCount * CryptoArbitrageThresholds.UnevenAmountRatioMinRange);

            for(int i = 0; i < GeneratorThresholds.Frequency.CryptoTransferTransactionsCount; i++)
            {
                var txType = random.NextDouble() < 0.5 ? TransactionType.P2p : TransactionType.IntrabankTransfer;
                var txDirection = _utilities.GetRandomTransactionDirection(txType, random);
                bool isCredit = txDirection == TransactionDirection.Credit;
                decimal txAmount = 0;

                if(i < unevenAmountsCounter)
                {
                    txAmount = random.Next(GeneratorThresholds.Amount.CryptoUnevenBaseMinRange, GeneratorThresholds.Amount.CryptoUnevenBaseMaxRange) * 100 + random.Next(1, 99);

                }
                else
                {
                    txAmount = random.Next(GeneratorThresholds.Amount.CryptoTransferMinRange, GeneratorThresholds.Amount.CryptoTransferMaxRange);
                }
                
                
                var txDate = _utilities.GenerateTransactionsDateTime(random, 12, 22, startWindow, endWindow);
                var txDetails = _utilities.GetTransactionDetailsBasedOnType(txType, isCredit, account, random);

                var counterparty = i < counterpartyPool.Count
                    ? counterpartyPool[i]
                    : counterpartyPool[random.Next(counterpartyPool.Count)];

                transactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = txType,
                    Direction = txDirection,
                    Status = statuses[random.Next(statuses.Count)],
                    Amount = txAmount,
                    AmountKzt = _utilities.ConvertToKzt(txAmount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = txDate,
                    TransactionCountryId = null,  
                    FromAccountId = txDetails.FromId,   
                    FromAccountNumber = txDetails.FromAccount,
                    FromCardNumber = txDetails.FromCard,
                    ToAccountId = txDetails.ToId,    
                    ToAccountNumber = txDetails.ToAccount,
                    ToCardNumber = txDetails.ToCard,
                    CounterpartyBank = txDetails.CounterpartyBank,
                    CounterpartyCountryId = counterparty.CountryId,  
                    MerchantId = null,  
                    MerchantName = null, 
                    CounterpartyName = counterparty.CounterpartyName,

                    IsFlagged = false
                });


            }



            // PHASE 2: RETAIL
            for(int j = 0; j < GeneratorThresholds.Frequency.CryptoRetailTransactionsCount; j++)
            {
                var merchant = j < CryptoArbitrageThresholds.NonAifcCryptoExchangeTransactionCountLast30DaysMinRange
                    ? reference.NonAifcCryptoExchanges[j]
                    : reference.CryptoExchanges[random.Next(reference.CryptoExchanges.Count)];

                var txDate = _utilities.GenerateTransactionsDateTime(random, 5, 14, startWindow, endWindow);
                decimal txAmount = random.Next(GeneratorThresholds.Amount.CryptoRetailMinRange, GeneratorThresholds.Amount.CryptoRetailMaxRange); 
                var txDetails = _utilities.GetTransactionDetailsBasedOnType(TransactionType.Retail, false, account, random);

                transactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.Retail,
                    Direction = TransactionDirection.Debit,
                    Status = statuses[random.Next(statuses.Count)],
                    Amount = txAmount,
                    AmountKzt = _utilities.ConvertToKzt(txAmount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = txDate,
                    TransactionCountryId = null,   
                    FromAccountId = txDetails.FromId,   
                    FromAccountNumber = txDetails.FromAccount,
                    FromCardNumber = txDetails.FromCard,
                    ToAccountId = txDetails.ToId,    
                    ToAccountNumber = txDetails.ToAccount,
                    ToCardNumber = txDetails.ToCard,
                    CounterpartyBank = txDetails.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = merchant.Name, 
                    CounterpartyName = null,

                    IsFlagged = false
                });
            }


            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList>());
        }
    }
}

