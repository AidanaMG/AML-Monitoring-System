using Aml.Application.Rules;
using Aml.Domain.Enums;
using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;

namespace Aml.Tests
{
    public class HighRiskCountryRuleTests
    {
        [Fact]
        public void Evaluate_OneHighCountry_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.High, false),
                CounterpartyCountryRisk = null,
                MerchantCountryRisk = null,
                
                ClientActivity = new ClientActivity
                {
                    TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days = 1
                }
                
            };


            var rule = new HighRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);

        }


        [Fact]
        public void Evaluate_ThreeHighCountry_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.High, false),
                CounterpartyCountryRisk = new CountryRiskInfo(RiskLevel.High, false),
                MerchantCountryRisk = null,

                ClientActivity = new ClientActivity
                {
                    TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days = CriticalAndHighRiskThresholds.HighRiskCountryTransactions3dMinRange
                }
                
            };


            var rule = new HighRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);

        }

        [Fact]
        public void Evaluate_NoHighCountry_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                TransactionCountryRisk = new CountryRiskInfo(RiskLevel.Low, false),
                CounterpartyCountryRisk = new CountryRiskInfo(RiskLevel.Medium, false),
                MerchantCountryRisk = null,

                ClientActivity = new ClientActivity
                {
                    TransactionAndMerchantAndCounterpartyHighRiskCountryTotalCountLast3Days = 0
                }
                
            };


            var rule = new HighRiskCountryRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);

        }
    }
}
