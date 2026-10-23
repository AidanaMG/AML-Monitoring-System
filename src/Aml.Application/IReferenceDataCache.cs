using Aml.Application.RulesAbstractions;
using Aml.Domain.Entities;
using Aml.Domain.Enums;

namespace Aml.Application
{
    public interface IReferenceDataCache
    {
        Country? GetCountry(int id);
        Merchant? GetMerchant(int id);
        MccCode? GetMccCode(int id);
        Task RefreshAsync(CancellationToken ct);

        bool IsKnownCounterparty(CounterpartyType type, string? rawName);
        int HomeCountryId { get; }


        RuleSettings GetRuleSettings();
        IReadOnlyList<Rule> GetActiveRules();
    }
}

