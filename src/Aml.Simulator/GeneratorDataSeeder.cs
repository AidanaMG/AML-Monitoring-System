using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Infrastructure;
using Aml.Simulator.Profiles;
using Aml.Simulator.ProfilesAdditions;
using Microsoft.EntityFrameworkCore;


namespace Aml.Simulator
{
    public static class GeneratorDataSeeder
    {
        public static async Task SeedAsync(AmlDbContext db, SimulatorReferenceData reference, CancellationToken ct)
        {
            if(await db.Clients.AnyAsync(ct)) return; 

            var random = new Random(13);

            var generators = new List<IProfileGenerator>
            { 
                new CleanProfileGenerator(),
                new CryptoArbitrageProfileGenerator(),
                new DropperProfileGenerator(),
                new DrugProfileGenerator(),
                new GamblingProfileGenerator(),
                new SanctionHitProfileGenerator(),
                new SimpleRiskLevelProfileGenerator(RiskScenario.CriticalCountry),
                new SimpleRiskLevelProfileGenerator(RiskScenario.HighCountry),
                new SimpleRiskLevelProfileGenerator(RiskScenario.CriticalMerchant),
                new SimpleRiskLevelProfileGenerator(RiskScenario.HighMerchant),
                new SimpleRiskLevelProfileGenerator(RiskScenario.HighFrequency),
                new SimpleRiskLevelProfileGenerator(RiskScenario.CombinedScoring)

            };

            foreach(var generator in generators)
            {
                var profile = generator.Generate(random, reference);
                await SaveProfileAsync(db, profile, reference, ct);
            }
        }

        static async Task SaveProfileAsync(AmlDbContext db, GeneratedProfile profile, 
            SimulatorReferenceData reference, CancellationToken ct)
        {
            // CLIENT
            profile.Client.CountryId = reference.HomeCountryId;
            db.Clients.Add(profile.Client);
            await db.SaveChangesAsync(ct);

            // ACCOUNT
            profile.Account.ClientId = profile.Client.Id;
            db.Accounts.Add(profile.Account);
            await db.SaveChangesAsync(ct);

            // WATCHLIST
            foreach(var entry in profile.WatchlistEntries)
            {
                entry.ClientId = profile.Client.Id;
            }

            db.WatchLists.AddRange(profile.WatchlistEntries);

            // TRANSACTIONS
            foreach(var tx in profile.Transactions)
            {
                LinkTransaction(tx, profile.Account, reference); 
            }


            foreach(var tx in profile.Transactions)
            {
                tx.Status = TransactionStatus.Completed;
            }

            profile.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .First()
                .Status = TransactionStatus.Pending;

            db.Transactions.AddRange(profile.Transactions); 
            await db.SaveChangesAsync(ct);
        }


        static void LinkTransaction(Transaction tx, Account account, SimulatorReferenceData reference)
        {
            // ACCOUNT
            if(tx.Direction == TransactionDirection.Credit)
            {
                tx.ToAccountId = account.Id;
                tx.FromAccountId = null;
            }
            else
            {
                tx.FromAccountId = account.Id;
                tx.ToAccountId = null;
            }

            // MERCHANT
            if(tx.MerchantName is not null)
            {
                if(!reference.MerchantByName.TryGetValue(tx.MerchantName, out var merchant))
                    throw new InvalidOperationException($"Merchant {tx.MerchantName} not found in DB");

                tx.MerchantId = merchant.Id;

                tx.TransactionCountryId ??= merchant.CountryId;
            }
            else
            {
                tx.TransactionCountryId ??= reference.HomeCountryId;
            }


        }
    }
}
