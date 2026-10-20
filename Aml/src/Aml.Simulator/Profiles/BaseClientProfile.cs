using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;


namespace Aml.Simulator.Profiles
{
    public class BaseClientProfile
    {
       
        private readonly Utilities _utilities = new();

        // CLIENT
        public Client CreateClient(Random random, DateTime joinDate, RiskLevel riskLevel, PepStatus pepStatus, DateTime dob)
        {
            bool isFemale = _utilities.IsFemale(random);
            var firstName = _utilities.GenerateFirstName(isFemale, random);
            var lastName = _utilities.GenerateLastName(isFemale, random);
            var address = _utilities.GenerateAddress(random);
            var phoneNumber = _utilities.GeneratePhoneNumber(random);

            return new Client
            {
                Id = 0, 
                Name = firstName,
                Surname = lastName,
                Patronymic = random.NextDouble() > 0.1 ? _utilities.GeneratePatronymic(isFemale, random) : null,
                Iin = _utilities.GenerateIin(dob, isFemale, random),
                Status = ClientStatus.Active,
                DateOfBirth = dob,
                Citizenship = "Kazakhstan",
                CountryId = 0, 
                Address = address,
                PhoneNumber = phoneNumber,
                RiskLevel = riskLevel,
                JoinDate = joinDate,
                PepStatus = pepStatus,
                PepPosition = pepStatus == PepStatus.NonPep ? null : _utilities.pepPositions[random.Next(_utilities.pepPositions.Length)],
                PepSince = pepStatus == PepStatus.NonPep ? null : _utilities.GeneratePepSince(random),
                
                IsFlagged = false,
                ScoringValue = 0,
                ScoringRole = null

            };

        }


        // ACCOUNT
        public Account CreateAccount(Random random, DateTime clientJoinDate, decimal currentBalance)
        {
            var randomType = _utilities.GetRandomAccountType(random);
            var createdAt = _utilities.GenerateAccountCreatedAtDate(clientJoinDate, 10, random);

            return new Account
            {
                Id = 0, 
                ClientId = 0,   
                Type = randomType, 
                AccountNumber = _utilities.GenerateAccountNumber(random),
                Currency = _utilities.currencies[random.Next(_utilities.currencies.Length)],
                Balance = currentBalance,
                Status = AccountStatus.Active,
                CreatedAt = createdAt, 
                CardNumber = randomType == AccountType.Card ? _utilities.GenerateCardNumber(random) : null
            };

        }



        // TRANSACTIONS
        public List<Transaction> CreateBackgroundTransactions(Random random, Account account, DateTime startDate, DateTime endDate, int count, SimulatorReferenceData reference)
        {
            var transactions = new List<Transaction>();

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


            // SALARY
            bool hasSalaryOnThisAccount = random.NextDouble() < 0.75;
            if (hasSalaryOnThisAccount)
            {
                for (int s = 0; s < GeneratorThresholds.Frequency.SalaryCount; s++)
                {
                    var salaryDate = startDate.AddMonths(s).AddDays(random.Next(0, 10)).AddHours(random.Next(9, 18)); 
                    var (min, max) = _utilities.GetAmountRange(TransactionType.Salary, account.Currency);
                    var salaryAmount = Math.Round(min + (decimal)random.NextDouble() * (max - min), 2); 

                    transactions.Add(new Transaction
                    {
                        Id = 0,
                        Type = TransactionType.Salary,
                        Direction = TransactionDirection.Credit,
                        Status = TransactionStatus.Completed,
                        Amount = salaryAmount,
                        AmountKzt = _utilities.ConvertToKzt(salaryAmount, account.Currency, reference.RatesToKzt),
                        Currency = account.Currency,
                        TransactionDate = salaryDate,
                        TransactionCountryId = null,  

                        FromAccountId = null,
                        FromAccountNumber = null,
                        FromCardNumber = null,

                        ToAccountId = account.Id,
                        ToAccountNumber = account.AccountNumber,
                        ToCardNumber = account.CardNumber, 

                        CounterpartyBank = _utilities.banks[random.Next(_utilities.banks.Length)],
                        CounterpartyCountryId = null,  
                        MerchantId = null,
                        MerchantName = null,

                        CounterpartyName = _utilities.employerNames[random.Next(_utilities.employerNames.Length)],
                        IsFlagged = false
                    });
                }
            }


            // OTHER TRANSACTIONS
            for (int i = 0; i < count; i++)
            {
                var txDate = _utilities.GenerateTransactionsDateTime(random, 8, 22, startDate, endDate);
                var randomTxType = weightedTypes[random.Next(weightedTypes.Count)];
                var randomTxStatus = weightedStatuses[random.Next(weightedStatuses.Count)];
                var randomDirection = _utilities.GetRandomTransactionDirection(randomTxType, random);

                var isCredit = randomDirection == TransactionDirection.Credit;

                var transactionDetailsBasedOnType = _utilities.GetTransactionDetailsBasedOnType(randomTxType, isCredit, account, random);
                var range = _utilities.GetAmountRange(randomTxType, account.Currency);
                decimal amount = Math.Round(range.Min + (decimal)random.NextDouble() * (range.Max - range.Min), 2);
                decimal amountKzt = _utilities.ConvertToKzt(amount, account.Currency, reference.RatesToKzt);


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

            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return transactions;
        }

    }
}
