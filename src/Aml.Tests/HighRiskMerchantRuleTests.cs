using Aml.Domain.Enums;
using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class HighRiskMerchantRuleTests
    {
        [Fact]
        public void Evaluate_HighMerchantsLastHourAboveMinRange_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = RiskLevel.High,

                ClientActivity = new ClientActivity
                {
                    HighRiskMerchantCountLastHour = 11
                }
                
            };

            var rule = new HighRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);
        }


        [Fact]
        public void Evaluate_OneHighMerchantLastHour_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = RiskLevel.High,

                ClientActivity = new ClientActivity
                {
                    HighRiskMerchantCountLastHour = 1
                }
                
            };

            var rule = new HighRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_NoHighMerchantLastHour_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = RiskLevel.Low,

                ClientActivity = new ClientActivity
                {
                    HighRiskMerchantCountLastHour = 0
                }
                
            };

            var rule = new HighRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_HighMerchantsLastHourAboveMinRange_CurrentTransactionNotMerchant_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                MerchantCategoryRiskLevel = null,

                ClientActivity = new ClientActivity
                {
                    HighRiskMerchantCountLastHour = 10
                }
                
            };

            var rule = new HighRiskMerchantRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);
        }

        
    }
}
