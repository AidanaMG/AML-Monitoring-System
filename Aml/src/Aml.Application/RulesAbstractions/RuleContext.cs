using Aml.Domain.Entities;
using Aml.Domain.Enums;


namespace Aml.Application.RulesAbstractions
{
    public sealed class RuleContext
    {
        public Transaction Transaction { get; init; } = null!;
        public int TriggeredAccountId { get; init; }


        public CountryRiskInfo? TransactionCountryRisk { get; init; }
        public CountryRiskInfo? CounterpartyCountryRisk { get; init; }
        public CountryRiskInfo? MerchantCountryRisk { get; init; }
        public RiskLevel? MerchantCategoryRiskLevel { get; init; }


        public ClientFacts ClientFacts { get; init; } = new();
        public ClientActivity ClientActivity { get; init; } = new();
        public AccountActivity TriggeredAccount { get; init; } = null!;
        public IReadOnlyList<AccountActivity> Accounts { get; init; } = new List<AccountActivity>();


        public RuleSettings Settings { get; init; } = new();
        public IReadOnlySet<string> ActiveRuleCodes { get; init; } = new HashSet<string>();
    }
}

