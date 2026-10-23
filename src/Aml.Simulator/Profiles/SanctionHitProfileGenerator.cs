using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Simulator.ProfilesAdditions;

namespace Aml.Simulator.Profiles
{
    public class SanctionHitProfileGenerator : IProfileGenerator
    {
        public string ProfileName => "SANCTION_HIT_PROFILE";

        private readonly BaseClientProfile _baseClient = new();
        private readonly Utilities _utilities = new();

        public GeneratedProfile Generate(Random random, SimulatorReferenceData reference)
        {
            var joinDate = DateTime.Now.AddMonths(-random.Next(GeneratorThresholds.Datetime.ClientJoinDateMinMonths, GeneratorThresholds.Datetime.ClientJoinDateMaxMonths));
            var dob = _utilities.GenerateDateOfBirth(random, 18, 65);
            var client = _baseClient.CreateClient(random, joinDate, RiskLevel.Critical, PepStatus.NonPep, dob);

            var currentBalance = Math.Round((decimal)(random.NextDouble() * GeneratorThresholds.Amount.AccountBalanceMaxRange), 2);
            var account = _baseClient.CreateAccount(random, client.JoinDate, currentBalance);

            var startDate = DateTime.Now.AddMonths(-GeneratorThresholds.Datetime.BackgroundTransactionsStartDateMonths); 
            var transactions = _baseClient.CreateBackgroundTransactions(random, account, startDate, DateTime.Now, GeneratorThresholds.Frequency.BackgroundTransactionsCount, reference);

            var watchlistEntry = new WatchList
            {
                ClientId = 0, 
                ListType = ListType.WorldSanctions, 
                Source = "OFAC",
                Reason = "Sanctions list match",
                AddedDate = DateTime.Now,
                AddedBy = "U.S. Department of the State"
            };

            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();
            return new GeneratedProfile(client, account, transactions, new List<WatchList> { watchlistEntry });
        }


    }
}
