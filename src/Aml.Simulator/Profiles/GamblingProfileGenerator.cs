using Aml.Application.Screening;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;

namespace Aml.Simulator.Profiles
{
    public class GamblingProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "GAMBLING_PROFILE";

        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();
        

        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.ClientJoinDateMinMonths, GeneratorThresholds.Datetime.ClientJoinDateMaxMonths));
            var dob = _utilities.GenerateDateOfBirth(random, 18, 60);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Low, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMaxRange), 2);
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);


            var transactions = new List<Transaction>();
            var weightedStatuses = _utilities.GetStandardWeightedStatuses();

            var activityDate = joinDate.AddDays(random.Next(GeneratorThresholds.Datetime.GamblingTransactionsStartWindowAfterJoinDate, GeneratorThresholds.Datetime.GamblingTransactionsEndWindowAfterJoinDate));
            var dayStart = activityDate.Date;
            var dayEnd = dayStart.AddDays(1);

            // DOUBLING PAIR DATA
            var doublingMerchantName = reference.GamblingMerchants[random.Next(reference.GamblingMerchants.Count)].Name;
            
            decimal firstDoubleAmount = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.SmallAmountMaxMultiplier + GeneratorThresholds.Amount.SmallAmountMinRange), 2); 
            decimal secondDoubleAmount = Math.Round(firstDoubleAmount * 2, 2);
            DateTime? firstDoubleDate = null;

            var payoutFromMerchantsList = new List<string>();


            // PHASE 1
            for(int i = 0; i < GamblingThresholds.RetailTransactionCountLast30DaysMinRange; i++)
            {
                decimal smallAmount = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.SmallAmountMaxMultiplier + GeneratorThresholds.Amount.SmallAmountMinRange), 2); 
                decimal largeAmount = Math.Round((decimal)(GeneratorThresholds.Amount.LargeAmountMinRange + random.NextDouble() * GeneratorThresholds.Amount.LargeAmountMaxMultiplier), 2);

                var transactionDetailsBasedOnTypeDebet = _utilities.GetTransactionDetailsBasedOnType(TransactionType.Retail, false, account, random);

                string merchantName;
                decimal currentAmount;
                DateTime currentTxDate;

                if(i == 0)
                {
                    currentAmount = firstDoubleAmount;
                    currentTxDate = _utilities.GenerateTransactionsDateTime(random, GeneratorThresholds.Datetime.GamblingStartHour, GeneratorThresholds.Datetime.GamblingStartHour + 1, dayStart, dayEnd);
                    firstDoubleDate = currentTxDate;
                    merchantName = doublingMerchantName;

                    payoutFromMerchantsList.Add(merchantName);
                }
                else if(i == 1)
                {
                    currentAmount = secondDoubleAmount;
                    currentTxDate = firstDoubleDate!.Value.AddMinutes(random.Next(GeneratorThresholds.Datetime.DoublingAmountMinMinutes, GamblingThresholds.MinutesDifferenceBetweenTransactionsMaxRange)); 
                    merchantName = doublingMerchantName;

                    payoutFromMerchantsList.Add(merchantName);

                }
                else
                {
                    currentAmount = random.NextDouble() > 0.3 ? smallAmount : largeAmount;
                    currentTxDate = _utilities.GenerateTransactionsDateTime(random, 12, 14, dayStart, dayEnd);
                    merchantName = reference.GamblingMerchants[random.Next(reference.GamblingMerchants.Count)].Name;

                    payoutFromMerchantsList.Add(merchantName);
                }



                transactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.Retail,
                    Direction = TransactionDirection.Debit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],
                    Amount = currentAmount,
                    AmountKzt = _utilities.ConvertToKzt(currentAmount, "USD", reference.RatesToKzt),
                    Currency = "USD",
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   
                    FromAccountId = transactionDetailsBasedOnTypeDebet.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnTypeDebet.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnTypeDebet.FromCard,
                    ToAccountId = transactionDetailsBasedOnTypeDebet.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnTypeDebet.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnTypeDebet.ToCard,
                    CounterpartyBank = transactionDetailsBasedOnTypeDebet.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = merchantName, 
                    CounterpartyName = null,

                    IsFlagged = false
                });
            }

            // PHASE 2: PAYOUT
            for(int j = 0; j < GeneratorThresholds.Frequency.GamblingPayoutTransactionsCount; j++)
            {
                decimal smallPayoutAmount = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.SmallAmountMaxMultiplier + GeneratorThresholds.Amount.SmallAmountMinRange), 2); 
                decimal largePayoutAmount = Math.Round((decimal)(GeneratorThresholds.Amount.LargeAmountMinRange + random.NextDouble() * GeneratorThresholds.Amount.LargeAmountPayoutMaxMultiplier), 2); 
                decimal currentAmount = random.NextDouble() > 0.2 ? smallPayoutAmount : largePayoutAmount;

                DateTime currentTxDate = _utilities.GenerateTransactionsDateTime(random, 15, 17, dayStart, dayEnd);

                var transactionDetailsBasedOnTypeCredit = _utilities.GetTransactionDetailsBasedOnType(TransactionType.CrossborderWireTransfer, true, account, random);

                transactions.Add(new Transaction
                {
                    Id = 0, 
                    Type = TransactionType.CrossborderWireTransfer,
                    Direction = TransactionDirection.Credit,
                    Status = weightedStatuses[random.Next(weightedStatuses.Count)],
                    Amount = currentAmount,
                    AmountKzt = _utilities.ConvertToKzt(currentAmount, "USD", reference.RatesToKzt),
                    Currency = "USD",
                    TransactionDate = currentTxDate,
                    TransactionCountryId = null,   
                    FromAccountId = transactionDetailsBasedOnTypeCredit.FromId,   
                    FromAccountNumber = transactionDetailsBasedOnTypeCredit.FromAccount,
                    FromCardNumber = transactionDetailsBasedOnTypeCredit.FromCard,
                    ToAccountId = transactionDetailsBasedOnTypeCredit.ToId,    
                    ToAccountNumber = transactionDetailsBasedOnTypeCredit.ToAccount,
                    ToCardNumber = transactionDetailsBasedOnTypeCredit.ToCard,
                    CounterpartyBank = transactionDetailsBasedOnTypeCredit.CounterpartyBank,
                    CounterpartyCountryId = null,  
                    MerchantId = null,  
                    MerchantName = null, 
                    CounterpartyName = payoutFromMerchantsList[random.Next(payoutFromMerchantsList.Count)],

                    IsFlagged = false
                });
            }


            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList>());

        }

    }
}