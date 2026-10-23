using Aml.Domain.Enums;
using Aml.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Aml.Simulator
{
    public class SimulatorReferenceData
    {
        public int HomeCountryId { get; init; }
        public int[] NonHomeCountryIds { get; init; } = null!;

        public int[] CriticalRiskCountryIds { get; init; } = null!;
        public int[] HighRiskCountryIds { get; init; } = null!;

        public string[] AntifraudCounterpartyNames { get; init; } = null!;

        public List<MerchantRef> CryptoExchanges { get; init; } = null!;
        public List<MerchantRef> GamblingMerchants { get; init; } = null!;

        public List<MerchantRef> CriticalRiskMerchants { get; init; } = null!;
        public List<MerchantRef> HighRiskMerchants { get; init; } = null!;

        public Dictionary<string, MerchantRef> MerchantByName { get; init; } = null!;

        public List<MerchantRef> GeneralMerchants { get; init; } = null!;

        public List<MerchantRef> NonAifcCryptoExchanges { get; init; } = null!;
        public List<MerchantRef> AifcCryptoExchanges { get; init; } = null!;

        public Dictionary<string, decimal> RatesToKzt { get; init; } = null!;

        public static async Task<SimulatorReferenceData> LoadAsync(AmlDbContext db, string homeCode, CancellationToken ct)
        {
            var countries = await db.Countries.AsNoTracking().ToListAsync(ct);
            
            var home = countries.First(c => c.Code == homeCode); 
            var nonHomeCountryIds = countries
                .Where(c => c.Id != home.Id && (c.RiskLevel == RiskLevel.Low || c.RiskLevel == RiskLevel.Medium))
                .Select(c => c.Id).ToArray();

            var criticalRiskCountriesIds = countries
                .Where(c => c.RiskLevel == RiskLevel.Critical)
                .Select(c => c.Id).ToArray();

            var highRiskCountriesIds = countries
               .Where(c => c.RiskLevel == RiskLevel.High)
               .Select(c => c.Id).ToArray();

            var merchants = await (from m in db.Merchants
                                   join mcc in db.MccCodes on m.MccId equals mcc.Id
                                   join c in db.Countries on m.CountryId equals (int?)c.Id
                                   select new { m.Id, m.Name, m.IsAifc, m.CountryId, 
                                       Category = mcc.Category,
                                       MccRiskLevel = mcc.RiskLevel,
                                       CountryRiskDetailsTag = c.RiskDetails,
                                       CountryRiskLevel = c.RiskLevel}).AsNoTracking().ToListAsync(ct); 

            var antifraudList = await db.KnownCounterparties.AsNoTracking()
                .Where(k => k.Type == CounterpartyType.AntifraudListed && k.RemovedDate == null)
                .Select(k => k.Name).ToArrayAsync(ct); 

            var cryptoExchangesMerchants = merchants
                .Where(m => m.IsAifc != null)
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList(); 

            var gamblingMerchants = merchants
                .Where(g => g.CountryRiskDetailsTag == CountryRiskTag.GamblingAndCasino)
                .Select(g => new MerchantRef(g.Id, g.Name, g.Category, g.IsAifc, g.CountryId))
                .ToList();

            var criticalRiskMerchants = merchants
                .Where(m => m.MccRiskLevel == RiskLevel.Critical)
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList();

            var highRiskMerchants = merchants
                .Where(m => m.MccRiskLevel == RiskLevel.High)
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList();

            var merchantByName = merchants.ToDictionary(
                r => r.Name,
                r => new MerchantRef(r.Id, r.Name, r.Category, r.IsAifc, r.CountryId));


            var generalMerchants = merchants
                .Where(m => m.IsAifc == null && m.CountryRiskDetailsTag == null
                && m.MccRiskLevel != RiskLevel.Critical
                && (m.CountryRiskLevel == RiskLevel.Low || m.CountryRiskLevel == RiskLevel.Medium)) 
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList();

            var nonAifcCryptoExchanges = merchants
                .Where(n => n.IsAifc == false)
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList();

            var aifcCryptoExchanges = merchants
                .Where(n => n.IsAifc == true)
                .Select(m => new MerchantRef(m.Id, m.Name, m.Category, m.IsAifc, m.CountryId))
                .ToList();

            var rates = await db.ExchangeRates.AsNoTracking()
                .ToDictionaryAsync(r => r.Currency, r => r.RateToKzt, ct);

            var data = new SimulatorReferenceData
            {
                HomeCountryId = home.Id,
                NonHomeCountryIds = nonHomeCountryIds,
                CriticalRiskCountryIds = criticalRiskCountriesIds,
                HighRiskCountryIds = highRiskCountriesIds,
                AntifraudCounterpartyNames = antifraudList,
                CryptoExchanges = cryptoExchangesMerchants,
                GamblingMerchants = gamblingMerchants,
                CriticalRiskMerchants = criticalRiskMerchants,
                HighRiskMerchants = highRiskMerchants,
                MerchantByName = merchantByName,
                GeneralMerchants = generalMerchants,
                NonAifcCryptoExchanges = nonAifcCryptoExchanges,
                AifcCryptoExchanges = aifcCryptoExchanges,
                RatesToKzt = rates
            };


            if(data.AntifraudCounterpartyNames.Length == 0)
                throw new InvalidOperationException("Antifraud counterparties not seeded");
            if(data.CryptoExchanges.Count == 0)
                throw new InvalidOperationException("Crypto exchanges not seeded");
            if(data.GamblingMerchants.Count == 0)
                throw new InvalidOperationException("Gambling merchants not seeded");
            if(data.NonHomeCountryIds.Length == 0)
                throw new InvalidOperationException("Non home country ids not seeded");
            if(data.CriticalRiskCountryIds.Length == 0)
                throw new InvalidOperationException("Critical risk level country ids not seeded");
            if(data.HighRiskCountryIds.Length == 0)
                throw new InvalidOperationException("High risk level country ids not seeded");
            if(data.CriticalRiskMerchants.Count == 0)
                throw new InvalidOperationException("Critical risk level merchants not seeded");
            if(data.HighRiskMerchants.Count == 0)
                throw new InvalidOperationException("High risk level merchants not seeded");
            if(data.GeneralMerchants.Count == 0)
               throw new InvalidOperationException("General merchants not seeded");
            if(data.NonAifcCryptoExchanges.Count == 0)
               throw new InvalidOperationException("Non aifc crypto exchanges not seeded");
            if(data.AifcCryptoExchanges.Count == 0)
                throw new InvalidOperationException("Aifc crypto exchanges not seeded");
            if (data.RatesToKzt.Count == 0)
                throw new InvalidOperationException("Exchange rates not seeded");

           

            return data;

        }

    }

    

    public record MerchantRef(int Id, string Name, string Category, bool? IsAifc, int? CountryId);

    
}
