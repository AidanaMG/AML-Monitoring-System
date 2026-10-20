using Aml.Domain.Entities;
using Aml.Domain.Enums;


namespace Aml.Simulator
{
    public class Utilities
    {
        // DATA
        public readonly string[] firstNamesMale = ["Adam", "Alisher", "Alexander", "Maxim", "Takhir", "Dmitry", "Timofey", "Egor", "Artem", "Zhandos", "Alan", "Timur"];
        public readonly string[] firstNamesFemale = ["Aigerim", "Dina", "Diana", "Alexandra", "Inna", "Victoria", "Alina", "Yana", "Maria", "Arina", "Ariana"];
        public readonly string[] lastNames = ["Akhmetov", "Kim", "Pak", "Petrov", "Asanov", "Omarov", "Aliev", "Miller", "Harris", "Volkov", "Lee", "Berger"];
        public readonly string[] patronymicBases = ["Aman", "Alan", "Maxim", "Viktor", "Oleg"];

        public readonly string[] cities = ["Almaty", "Astana", "Shymkent", "Aktobe", "Taraz", "Semey", "Atyrau", "Karaganda", "Ust-Kamenogorsk"];
        public readonly string[] streets = ["Abaya Ave.", "Tauke Khana Ave.", "Republic Ave.", "Momyshuly St.", "Tole Bi St.", "Kazybek Bi St.", "Aiteke Bi St.", "Mangilik El Ave.", "Kunaeva St.", "Ryskulova St.", "Zhandosova St."];

        public readonly string[] currencies = ["KZT", "KZT", "KZT", "USD", "EUR"];

        public readonly string[] employerNames = ["LLP Norsec Delta Projects", "LLP AtyrauTechInvest", "IE Nurlan Trading", "JSC KazTransOil", "LLP BI Group"];
        public readonly string[] banks = ["Kaspi Bank", "Halyk Bank", "Forte Bank", "Jusan Bank"];
        public readonly string[] worldBanks = ["JPMorgan Chase & Co.", "Bank of America (BofA)", "HSBC", "Deutsche Bank"];

        public readonly string[] pepPositions = ["Head of Government", "Minister of Finance", "Deputy Minister", "Supreme Court Justice", "Prosecutor General", "Head of National Police", "Central Bank Governor", "Member of the Mazhilis"];

        public readonly string[] counterpartyNames = ["Maxim", "Egor", "Milana", "Victoria", "Aliya", "Takhir", "Aman", "Yana", "Alan", "Daniyar", "Aidana"];
        public readonly string[] counterpartyAbbreviations = ["A.B.", "B.T.", "T.A.", "L.N.", "E.O.", "A.A.", "B.B.", "R.K.", "D.D.", "R.A."];
        
       
        public decimal ConvertToKzt(decimal amount, string currency, Dictionary<string, decimal> rates)
        {
           
            return Math.Round(amount * rates.GetValueOrDefault(currency, 1m), 2);
        }

        public (decimal Min, decimal Max) GetAmountRange(TransactionType type, string currency) 
        {

            switch(currency)
            {
                case "KZT":
                    var resultKzt = type switch
                    {
                        TransactionType.Salary => (150_000m, 1_000_000m),
                        TransactionType.Retail => (500m, 200_000m),
                        TransactionType.Atm => (10_000m, 500_000m),
                        TransactionType.P2p => (100m, 500_000m),
                        TransactionType.IntrabankTransfer => (100m, 500_000m),
                        TransactionType.CrossborderWireTransfer => (100_000m, 1_000_000m),

                        _ => throw new ArgumentOutOfRangeException(nameof(type))
                    };
                    return resultKzt;


                default:
                    var resultUsdOrEur = type switch
                    {
                        TransactionType.Salary => (1000m, 3000m),
                        TransactionType.Retail => (10m, 500m),
                        TransactionType.Atm => (100m, 1000m),
                        TransactionType.P2p => (10m, 1000m),
                        TransactionType.IntrabankTransfer => (10m, 1000m),
                        TransactionType.CrossborderWireTransfer => (250m, 2_200m),
                        _ => throw new ArgumentOutOfRangeException(nameof(type))
                    };
                    return resultUsdOrEur;
            }
        }

        public string GenerateIin(DateTime dob, bool isFemale, Random random)
        {
            string yy = dob.Year.ToString()[2..];
            string mm = dob.Month.ToString("D2");
            string dd = dob.Day.ToString("D2");

            string tempCentury;
            if (dob.Year < 2000) { tempCentury = isFemale ? "4" : "3"; } else { tempCentury = isFemale ? "6" : "5"; }
            string century = tempCentury;

            string tail = random.Next(10000, 99999).ToString();
            return $"{yy}{mm}{dd}{century}{tail}";
        }

        public string GeneratePatronymic(bool isFemale, Random random)
        {
            string base_ = patronymicBases[random.Next(patronymicBases.Length)];
            return isFemale ? base_ + "ovna" : base_ + "ovich";
        }

        public string GenerateCounterpartyName(Random random)
        {
            return $"{counterpartyNames[random.Next(counterpartyNames.Length)]} {counterpartyAbbreviations[random.Next(counterpartyAbbreviations.Length)]}";
        }
        
        public string GenerateAccountNumber(Random random)
        {
            return $"KZ{random.Next(10000, 99999)}Z{random.Next(100000000, 999999999)}{random.Next(100, 999)}";
        }

        public string GenerateCardNumber(Random random)
        {
            return $"{random.Next(1000, 9999)}********{random.Next(1000, 9999)}";
        }


        public bool IsFemale(Random random)
        {
            bool isFemale = random.NextDouble() > 0.5;
            return isFemale;

        }

        public string GenerateFirstName(bool isFemale, Random random)
        {
            string firstName = isFemale
               ? firstNamesFemale[random.Next(firstNamesFemale.Length)]
               : firstNamesMale[random.Next(firstNamesMale.Length)];

            return firstName;
        }

        public string GenerateLastName(bool isFemale, Random random)
        {
            string lastName = lastNames[random.Next(lastNames.Length)];
            if (isFemale && lastName.EndsWith("v")) lastName += "a";

            return lastName;

        }


        public DateTime GenerateDateOfBirth(Random random, int start, int end)
        {
            return DateTime.Now.AddYears(-random.Next(start, end + 1)).AddDays(-random.Next(0, 365));

        }

        public DateTime GeneratePepSince(Random random)
        {
            return DateTime.Now.AddYears(-random.Next(1, 10));

        }

        public string GenerateAddress(Random random)
        {
            return $"City: {cities[random.Next(cities.Length)]}, {streets[random.Next(streets.Length)]}, {random.Next(1, 200)}";
        }

        public string GeneratePhoneNumber(Random random)
        {
            return $"+7{random.Next(700, 778)}{random.Next(1000000, 9999999)}";
        }

        public AccountType GetRandomAccountType(Random random)
        {
            var accountTypeValues = Enum.GetValues<AccountType>();
            return accountTypeValues[random.Next(accountTypeValues.Length)];
        }

        public DateTime GenerateAccountCreatedAtDate(DateTime clientJoinDate, int days, Random random)
        {
            return clientJoinDate.AddDays(random.Next(0, days));
        }

        public DateTime GenerateTransactionsDateTime(Random random, int startHour, int endHour, DateTime startDate, DateTime endDate)
        {
            if(endDate <= startDate) return startDate;

            var txDate = startDate.AddSeconds(random.Next(0, (int)(endDate - startDate).TotalSeconds));
            txDate = txDate.Date.AddHours(random.Next(startHour, endHour)).AddMinutes(random.Next(0, 60)).AddSeconds(random.Next(0, 60));

            if(txDate < startDate || txDate > endDate)
            {
                txDate = startDate.AddSeconds(random.Next(0, (int)(endDate - startDate).TotalSeconds));
            }

            return txDate;
        }

        public TransactionDirection GetRandomTransactionDirection(TransactionType randomTxType, Random random)
        {
            TransactionDirection randomDirection;
            
            if(randomTxType != TransactionType.Retail && randomTxType != TransactionType.Salary)
            {
                var txDirectionValues = Enum.GetValues<TransactionDirection>();
                randomDirection = txDirectionValues[random.Next(txDirectionValues.Length)];
            }
            else if(randomTxType == TransactionType.Salary)
            {
                randomDirection = TransactionDirection.Credit;
            } 
            else
            {
                randomDirection = TransactionDirection.Debit;
            }

            return randomDirection;

        }


        public bool IsRetail(TransactionType txType)
        {
            return txType == TransactionType.Retail;
        }


        public record TransactionDetails(
            int? FromId,
            int? ToId,
            string? FromAccount,
            string? ToAccount,
            string? FromCard,
            string? ToCard,
            string? CounterpartyBank

        );


        public TransactionDetails GetTransactionDetailsBasedOnType(TransactionType randomTxType, bool isCredit, Account account, Random random)
        {
            int? fromId = null;
            int? toId = null;
            string? fromAccount = null;
            string? toAccount = null;
            string? fromCard = null;
            string? toCard = null;
            string? counterpartyBank = null;
            

            switch(randomTxType) 
            {
                case TransactionType.P2p:
                    fromId = isCredit ? null : account.Id;
                    toId = isCredit ? account.Id : null;
                    fromAccount = null;
                    toAccount = null;
                    fromCard = isCredit ? GenerateCardNumber(random) : account.CardNumber;
                    toCard = isCredit ? account.CardNumber : GenerateCardNumber(random);
                    counterpartyBank = $"{banks[random.Next(banks.Length)]}";
                    break;

                case TransactionType.IntrabankTransfer:
                    fromId = isCredit ? null : account.Id; 
                    toId = isCredit ? account.Id : null;
                    fromAccount = isCredit ? GenerateAccountNumber(random) : account.AccountNumber; 
                    toAccount = isCredit ? account.AccountNumber : GenerateAccountNumber(random); 
                    fromCard = null;
                    toCard = null;
                    counterpartyBank = $"Freedom Bank";
                    break;

                case TransactionType.CrossborderWireTransfer:
                    fromId = isCredit ? null : account.Id;
                    toId = isCredit ? account.Id : null;
                    fromAccount = isCredit ? GenerateAccountNumber(random) : account.AccountNumber;
                    toAccount = isCredit ? account.AccountNumber : GenerateAccountNumber(random);
                    fromCard = null;
                    toCard = null;
                    counterpartyBank = $"{worldBanks[random.Next(worldBanks.Length)]}";
                    break;

                case TransactionType.Retail:
                    fromId = account.Id;
                    toId = null;
                    fromAccount = account.AccountNumber;
                    toAccount = null;
                    fromCard = account.CardNumber;
                    toCard = null;
                    counterpartyBank = null;
                    break;

                case TransactionType.Atm:
                    fromId = isCredit ? null : account.Id;
                    toId = isCredit ? account.Id : null;
                    fromAccount = isCredit ? null : account.AccountNumber;
                    toAccount = isCredit ? account.AccountNumber : null;
                    fromCard = null;
                    toCard = null;
                    counterpartyBank = null;
                    break;
            }

            return new TransactionDetails(
                    fromId,
                    toId,
                    fromAccount,
                    toAccount,
                    fromCard,
                    toCard,
                    counterpartyBank
            );
        }


        public List<TransactionStatus> GetStandardWeightedStatuses()
        {
            var weightedStatuses = new List<TransactionStatus>();
            weightedStatuses.AddRange(Enumerable.Repeat(TransactionStatus.Completed, 100));

            return weightedStatuses;
        }
    }
}
