using Aml.Application.Rules;
using Aml.Domain.Enums;
using Aml.Application.RulesAbstractions;


namespace Aml.Tests
{
    public class CriticalRiskCountryRuleTests
    {
        [Fact]
        public void Evaluate_OneCriticalFatfCountry_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.Critical, true),
                CounterpartyCountryRisk = null,
                MerchantCountryRisk = null,
                
            };

            
            var rule = new CriticalRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);

        }

        [Fact]
        public void Evaluate_TwoCriticalOneFatfCountry_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.Critical, false),
                CounterpartyCountryRisk = new CountryRiskInfo(RiskLevel.Critical, true),
                MerchantCountryRisk = null
            };


            var rule = new CriticalRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);

        }


        [Fact]
        public void Evaluate_OneCriticalFatfOneHighCountry_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.Critical, true),
                CounterpartyCountryRisk = new CountryRiskInfo(RiskLevel.High, false),
                MerchantCountryRisk = null
            };


            var rule = new CriticalRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);

        }

        [Fact]
        public void Evaluate_NoCriticalFatfCountry_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.Low, false),
                CounterpartyCountryRisk = null,
                MerchantCountryRisk = new CountryRiskInfo(RiskLevel.Medium, false)
            };

            
            var rule = new CriticalRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);

        }

        [Fact]
        public void Evaluate_HighRiskFatfCountry_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.High, true),
                CounterpartyCountryRisk = null,
                MerchantCountryRisk = null
            };

            var rule = new CriticalRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }
    }
}
