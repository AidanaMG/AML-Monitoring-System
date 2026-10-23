using Aml.Application;
using Aml.Application.RulesAbstractions;
using Aml.Domain;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Aml.Infrastructure
{
    public class EfReferenceDataCache : IReferenceDataCache
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Dictionary<int, Country> _countries = new();
        private Dictionary<int, Merchant> _merchants = new();
        private Dictionary<int, MccCode> _mccCodes = new();
        private Dictionary<CounterpartyType, HashSet<string>> _counterpartiesByType = new();
        public int HomeCountryId { get; private set; }
        private readonly string _homeCountryCode;

        private RuleSettings _ruleSettings = new();
        private IReadOnlyList<Rule> _activeRules = new List<Rule>();
        
        public EfReferenceDataCache(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {            
            _scopeFactory = scopeFactory;

            var code = configuration["Aml:HomeCountryCode"];
            if(string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Aml:HomeCountryCode is not configured");
            _homeCountryCode = code;
              

        }


        public async Task RefreshAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AmlDbContext>();

            var dictionaryCountries = await db.Countries
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id, ct);
            var dictionaryMerchants = await db.Merchants
                .AsNoTracking()
                .ToDictionaryAsync(m => m.Id, ct);
            var dictionaryMccCodes = await db.MccCodes
                .AsNoTracking()
                .ToDictionaryAsync(mc => mc.Id, ct);

            var activeKnownCounterparties = await db.KnownCounterparties
                    .Where(c => c.RemovedDate == null)
                    .AsNoTracking()
                    .ToListAsync(ct);

            var dictionaryKnownCounterparties = activeKnownCounterparties.GroupBy(c => c.Type)
                       .ToDictionary(g => g.Key, g => g.Select(c => c.NormalizedName)
                       .ToHashSet(StringComparer.Ordinal));

            var homeCountry = await db.Countries
               .AsNoTracking()
               .FirstOrDefaultAsync(c => c.Code == _homeCountryCode, ct);

            if(homeCountry is null)
                throw new InvalidOperationException($"{_homeCountryCode} not found in Countries");


            var activeRules = await db.Rules
                .Where(r => r.IsActive)
                .AsNoTracking()
                .ToListAsync(ct);

            var settingsByRuleCode = new Dictionary<string, Dictionary<string, decimal>>();

            foreach(var activeRule in activeRules)
            {
                Dictionary<string, decimal> parameters;

                try 
                {
                    parameters = JsonSerializer.Deserialize<Dictionary<string, decimal>>(activeRule.ParametersJson)
                    ?? new Dictionary<string, decimal>(); 
                }
                catch(JsonException)
                {
                    parameters = new Dictionary<string, decimal>();
                }

                parameters["__weight"] = activeRule.Weight; 

                settingsByRuleCode[activeRule.RuleCode] = parameters;

            }


            _counterpartiesByType = dictionaryKnownCounterparties;
            _countries = dictionaryCountries;
            _merchants = dictionaryMerchants;
            _mccCodes = dictionaryMccCodes;
            HomeCountryId = homeCountry.Id;
            _ruleSettings = new RuleSettings(settingsByRuleCode);
            _activeRules = activeRules;

        }

        

        public Country? GetCountry(int id) => _countries.GetValueOrDefault(id);
        public Merchant? GetMerchant(int id) => _merchants.GetValueOrDefault(id);
        public MccCode? GetMccCode(int id) => _mccCodes.GetValueOrDefault(id);
        public bool IsKnownCounterparty(CounterpartyType type, string? rawName)
        {
            string normalizedName = CounterpartyName.Normalize(rawName);

            if(normalizedName.Length == 0) return false;

            if(!_counterpartiesByType.TryGetValue(type, out var names)) return false;

            return names.Contains(normalizedName);
        }

        public RuleSettings GetRuleSettings() => _ruleSettings;
        public IReadOnlyList<Rule> GetActiveRules() => _activeRules;

    }
}
