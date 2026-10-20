using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;

namespace Aml.Simulator.Profiles
{
    public class CleanProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "CLEAN_PROFILE";

        
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
            var transactions = _baseClient.CreateBackgroundTransactions(random, account, startDate, DateTime.Now, GeneratorThresholds.Frequency.BackgroundTransactionsCount, reference);

            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList>());
        }

    }
}

