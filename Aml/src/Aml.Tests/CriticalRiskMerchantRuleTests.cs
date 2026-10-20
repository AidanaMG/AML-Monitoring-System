using Aml.Application.Rules;
using Aml.Domain.Enums;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class CriticalRiskMerchantRuleTests
    {

        [Fact]
        public void Evaluate_OneCriticalMerchant_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = RiskLevel.Critical
            };

            var rule = new CriticalRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);

        }


        [Fact]
        public void Evaluate_NoCriticalMerchant_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = RiskLevel.Low
            };

            var rule = new CriticalRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_NoMerchant_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = null
            };

            var rule = new CriticalRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }
    }
}


