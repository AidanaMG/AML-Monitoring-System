using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;


namespace Aml.Simulator.Profiles
{
    public class DrugProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "DRUG_PROFILE";

        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();

        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.FreshClientJoinDateMinMonths, GeneratorThresholds.Datetime.FreshClientJoinDateMaxMonths));
            var dob = _utilities.GenerateDateOfBirth(random, DrugThresholds.ClientAgeMinRange, DrugThresholds.ClientAgeMaxRange);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Low, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMinRange), 2);
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);


            var transactions = new List<Transaction>();
            
            var endWindow = DateTime.Now;
            var startWindow24h = endWindow.AddHours(-GeneratorThresholds.Datetime.DrugStartWindow24h)
                .AddMinutes(DrugThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange); 

            var statuses = _utilities.GetStandardWeightedStatuses();

            List<(string CounterpartyName, string CounterpartyCardNumber)> counterpartyPool = new();
            var uniqueCounterpartyPool = new HashSet<(string CounterpartyName, string CounterpartyCardNumber)>();
            List<string> cardNumbers = new();
            var uniqueCardNumbers = new HashSet<string>();

            while(uniqueCardNumbers.Count < DrugThresholds.UniqueIncomingCardCountLast24hMinRange)
            {
                uniqueCardNumbers.Add(_utilities.GenerateCardNumber(random));
            }

            cardNumbers.AddRange(uniqueCardNumbers);

            while(uniqueCounterpartyPool.Count < DrugThresholds.UniqueIncomingCounterpartiesLast24hMinRange)
            {
                var card = cardNumbers[uniqueCounterpartyPool.Count % cardNumbers.Count];
                uniqueCounterpartyPool.Add((_utilities.GenerateCounterpartyName(random), card));
            }

            counterpartyPool.AddRange(uniqueCounterpartyPool);


            // PHASE 1: P2P 24H CREDIT
            var transactions24h = new List<Transaction>();
            decimal mostFrequent = 0;
            decimal txAmount = 0;

            for(int i = 0; i < DrugThresholds.IncomingP2pCountLast24hMinRange; i++)
            {
                decimal amount = Math.Round((decimal)(random.Next((int)DrugThresholds.DrugPriceMinRange, (int)DrugThresholds.DrugPriceMaxRange)), 2);
                
                if(i == 0)
                {
                    mostFrequent = amount;
                }
                if(i <= DrugThresholds.IncomingP2pCountLast24hMinRange / 2)
                {
                    txAmount = mostFrequent;

                }
                else
                {
                    txAmount = amount;

                }

                var counterparty = i < counterpartyPool.Count
                    ? counterpartyPool[i]
                    : counterpartyPool[random.Next(counterpartyPool.Count)];

                var txDate = _utilities.GenerateTransactionsDateTime(random, 1, 14, startWindow24h, endWindow);
                var txDetails = _utilities.GetTransactionDetailsBasedOnType(TransactionType.P2p, true, account, random);



                transactions24h.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.P2p,
                    Direction = TransactionDirection.Credit,
                    Status = statuses[random.Next(statuses.Count)],
                    Amount = txAmount,
                    AmountKzt = _utilities.ConvertToKzt(txAmount, account.Currency, reference.RatesToKzt),
                    Currency = account.Currency,
                    TransactionDate = txDate,
                    TransactionCountryId = null,   
                    FromAccountId = txDetails.FromId,   
                    FromAccountNumber = txDetails.FromAccount,
                    FromCardNumber = counterparty.CounterpartyCardNumber,
                    ToAccountId = txDetails.ToId,    
                    ToAccountNumber = txDetails.ToAccount,
                    ToCardNumber = txDetails.ToCard,
                    CounterpartyBank = txDetails.CounterpartyBank,
                    CounterpartyCountryId = null, 
                    MerchantId = null,  
                    MerchantName = null, 
                    CounterpartyName = counterparty.CounterpartyName,

                    IsFlagged = false
                });

            }


            // PHASE 2: WITHDRAWAL
            var withdrawalTx = new List<Transaction>();
            var lastIncomingDate = transactions24h.Max(t => t.TransactionDate);
            var withdrawalDate = lastIncomingDate.AddMinutes(random.Next(2, DrugThresholds.MinutesToFirstWithdrawalAfterIncomingMaxRange));
            decimal withdrawalAmount = transactions24h.Sum(a => a.Amount);
            var withdrawalTxDetails = _utilities.GetTransactionDetailsBasedOnType(TransactionType.P2p, false, account, random);
            

            withdrawalTx.Add(new Transaction
            {
                Id = 0, 
                Type = TransactionType.P2p,
                Direction = TransactionDirection.Debit,
                Status = statuses[random.Next(statuses.Count)],
                Amount = withdrawalAmount,
                AmountKzt = _utilities.ConvertToKzt(withdrawalAmount, account.Currency, reference.RatesToKzt),
                Currency = account.Currency,
                TransactionDate = withdrawalDate,
                TransactionCountryId = null,   
                FromAccountId = withdrawalTxDetails.FromId,   
                FromAccountNumber = withdrawalTxDetails.FromAccount,
                FromCardNumber = withdrawalTxDetails.FromCard,
                ToAccountId = withdrawalTxDetails.ToId,    
                ToAccountNumber = withdrawalTxDetails.ToAccount,
                ToCardNumber = withdrawalTxDetails.ToCard,
                CounterpartyBank = withdrawalTxDetails.CounterpartyBank,
                CounterpartyCountryId = null, 
                MerchantId = null,  
                MerchantName = null, 
                CounterpartyName = counterpartyPool[random.Next(counterpartyPool.Count)].CounterpartyName, 

                IsFlagged = false
            });


            // PHASE 3: REGULAR TRANSACTIONS 3 DAYS
            var regularity3daysTx = new List<Transaction>();
            
            for(int d = 0; d < DrugThresholds.DistinctDaysWithActivityMinRange; d++)
            {
                var dayStart = lastIncomingDate.AddDays(-(d + 2));
                var dayEnd = lastIncomingDate.AddDays(-(d + 1));
                
                for(int o = 0; o < DrugThresholds.IncomingAndOutcomingP2pCount3dMinRange / 2; o++)
                {
                    var regularityDate = _utilities.GenerateTransactionsDateTime(random, 10, 22, dayStart, dayEnd);

                    var regularityDetails = _utilities.GetTransactionDetailsBasedOnType(TransactionType.P2p, true, account, random);
                    decimal regularityAmount = Math.Round((decimal)(random.Next((int)DrugThresholds.DrugPriceMinRange, (int)DrugThresholds.DrugPriceMaxRange)), 2);


                    regularity3daysTx.Add(new Transaction
                    {
                        Id = 0, 
                        Type = TransactionType.P2p,
                        Direction = TransactionDirection.Credit,
                        Status = statuses[random.Next(statuses.Count)],
                        Amount = regularityAmount,
                        AmountKzt = _utilities.ConvertToKzt(regularityAmount, account.Currency, reference.RatesToKzt),
                        Currency = account.Currency,
                        TransactionDate = regularityDate,
                        TransactionCountryId = null,   
                        FromAccountId = regularityDetails.FromId,   
                        FromAccountNumber = regularityDetails.FromAccount,
                        FromCardNumber = regularityDetails.FromCard,
                        ToAccountId = regularityDetails.ToId,    
                        ToAccountNumber = regularityDetails.ToAccount,
                        ToCardNumber = regularityDetails.ToCard,
                        CounterpartyBank = regularityDetails.CounterpartyBank,
                        CounterpartyCountryId = null, 
                        MerchantId = null,  
                        MerchantName = null, 
                        CounterpartyName = counterpartyPool[random.Next(counterpartyPool.Count())].CounterpartyName,

                        IsFlagged = false
                    });
                }

                
            }



            var allTransactions = new List<Transaction>();
            allTransactions.AddRange(transactions24h);
            allTransactions.AddRange(withdrawalTx);
            allTransactions.AddRange(regularity3daysTx);

            allTransactions = allTransactions.OrderBy(t => t.TransactionDate).ToList();

            return new GeneratedProfile(client, account, allTransactions, new List<WatchList>());
        }

    }
}