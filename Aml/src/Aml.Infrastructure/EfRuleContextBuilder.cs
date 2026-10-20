using Aml.Application;
using Aml.Application.RulesAbstractions;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Aml.Infrastructure
{
    public class EfRuleContextBuilder : IRuleContextBuilder
    {
        private readonly AmlDbContext _db;
        private readonly IReferenceDataCache _dataCache;

        public EfRuleContextBuilder(AmlDbContext db, IReferenceDataCache dataCache)
        {
            _db = db;
            _dataCache = dataCache;
        }

        public async Task<RuleContext> BuildAsync(Transaction transaction, DateTime evaluationTime, CancellationToken ct)
        {

            var last1h = evaluationTime.AddHours(-1);
            var last24h = evaluationTime.AddHours(-24);
            var last3d = evaluationTime.AddDays(-3);
            var last30d = evaluationTime.AddDays(-30);


            var triggeredAccountId = transaction.FromAccountId ?? transaction.ToAccountId
                ?? throw new InvalidOperationException($"Transaction {transaction.Id} has no account");

            var triggeredAccount = await _db.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == triggeredAccountId, ct)
                ?? throw new InvalidOperationException($"No account with id {triggeredAccountId} in DB");

            var clientId = triggeredAccount.ClientId;


            var client = await _db.Clients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clientId, ct)
                ?? throw new InvalidOperationException($"No client with id {clientId} in DB");


            var accounts = await _db.Accounts.AsNoTracking()
                .Where(a => a.ClientId == clientId).ToListAsync(ct);

            var accountIds = accounts.Select(a => a.Id).ToHashSet();


            var watchlist = await _db.WatchLists.AsNoTracking()
                .Where(w => w.ClientId == clientId && w.RemovedDate == null).ToListAsync(ct);


            var transactions30d = await _db.Transactions.AsNoTracking()
                .Where(t => (t.FromAccountId != null && accountIds.Contains(t.FromAccountId.Value))
                || (t.ToAccountId != null && accountIds.Contains(t.ToAccountId.Value)))
                .Where(t => t.TransactionDate > last30d && t.TransactionDate <= evaluationTime)
                .Where(t => t.Status == TransactionStatus.Completed)
                .ToListAsync(ct);

            
            if(!transactions30d.Any(t => t.Id == transaction.Id))
                transactions30d.Add(transaction);


            var transactionCountryRisk = ComputeCountryRisk(transaction.TransactionCountryId);
            var counterpartyCountryRisk = ComputeCountryRisk(transaction.CounterpartyCountryId);

            var merchant = transaction.MerchantId is int mId
                ? _dataCache.GetMerchant(mId)
                : null;
            var merchantCountryRisk = ComputeCountryRisk(merchant?.CountryId);
            var merchantCategoryRisk = merchant is null
                ? null
                : _dataCache.GetMccCode(merchant.MccId)?.RiskLevel;

            var clientFacts = new ClientFacts
            {
                ActiveWatchlistEntries = watchlist,
                ClientAgeDays = ComputeClientAgeDays(evaluationTime, client.JoinDate),
                ClientAge = ComputeClientAge(evaluationTime, client.DateOfBirth),
                ClientId = client.Id
            };



            var accountActivities = accounts
                .Select(a => RuleContextCalculator.BuildAccountActivity(a, transactions30d, last24h, accountIds))
                .ToList();
            var triggeredAccountActivity = accountActivities.FirstOrDefault(a => a.AccountId == triggeredAccountId)
                ?? throw new InvalidOperationException($"Triggered transaction account id {triggeredAccountId} does not belong to a client"); 


            var clientActivity = RuleContextCalculator.BuildClientActivity(
                transactions30d, accountIds, last1h, last24h, last3d, _dataCache);


            var settings = _dataCache.GetRuleSettings();
            var activeRuleCodes = _dataCache.GetActiveRules()
                .Select(r => r.RuleCode)
                .ToHashSet();

            if(activeRuleCodes.Count == 0) 
                throw new InvalidOperationException("Active rule codes list cannot be empty");


            return new RuleContext { 
                Transaction = transaction,
                TriggeredAccountId = triggeredAccountId,

                TransactionCountryRisk = transactionCountryRisk,
                CounterpartyCountryRisk = counterpartyCountryRisk,
                MerchantCountryRisk = merchantCountryRisk,
                MerchantCategoryRiskLevel = merchantCategoryRisk,

                ClientFacts = clientFacts,
                ClientActivity = clientActivity,
                TriggeredAccount = triggeredAccountActivity,
                Accounts = accountActivities,
                
                Settings = settings,
                ActiveRuleCodes = activeRuleCodes

            };
        }

       
        private CountryRiskInfo? ComputeCountryRisk(int? countryId)
        {
            if(countryId is null) return null;

            var country = _dataCache.GetCountry(countryId.Value);
            return country is null ? null
                : new CountryRiskInfo(country.RiskLevel, country.FatfListed);
        }


        private static int ComputeClientAgeDays(DateTime evaluationTime, DateTime joinDate)
            => (evaluationTime - joinDate).Days;


        private static int ComputeClientAge(DateTime evaluationTime, DateTime dob)
        {
            int age = evaluationTime.Year - dob.Year;

            if (evaluationTime.Month < dob.Month
                || (evaluationTime.Month == dob.Month && evaluationTime.Day < dob.Day))
                age--;

            return age;
        }
        
    }
}