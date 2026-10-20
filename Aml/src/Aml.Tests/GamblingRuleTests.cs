using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class GamblingRuleTests
    {
        [Fact]
        public void Evaluate_GamblingRuleTriggeredAllChecks_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 2,
                    UniqueForeignMerchantTransactionCountLast24h = 20,
                    ForeignMerchantTransactionsLast24h = new List<(decimal Amount, DateTime Time)> {
                    (11.11m, new DateTime(2026, 8, 10, 14, 30, 10)),
                    (22.22m, new DateTime(2026, 8, 10, 14, 45, 50)),
                    (100m, new DateTime(2026, 8, 10, 14, 57, 30)),
                    (11.77m, new DateTime(2026, 8, 10, 15, 11, 20)),
                    (23.49m, new DateTime(2026, 8, 10, 15, 30, 17))},
                    MerchantCategoryCountLast24h = 5,
                    IncomingFromGamblingMerchantsCountLast24h = 3
                }
                //100
            };
            

            var rule = new GamblingRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }


        [Fact]
        public void Evaluate_GamblingRuleTriggeredSomeChecksScoringAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 22,
                    UniqueForeignMerchantTransactionCountLast24h = 3,
                    ForeignMerchantTransactionsLast24h = new List<(decimal Amount, DateTime Time)> {
                    (11.11m, new DateTime(2026, 8, 10, 14, 30, 10)),
                    (22.22m, new DateTime(2026, 8, 10, 14, 45, 50)),
                    (100m, new DateTime(2026, 8, 10, 14, 57, 30)),
                    (11.77m, new DateTime(2026, 8, 10, 15, 11, 20)),
                    (23.49m, new DateTime(2026, 8, 10, 15, 30, 17))},
                    MerchantCategoryCountLast24h = 5,
                    IncomingFromGamblingMerchantsCountLast24h = 1
                }
                //80
            };
            

            var rule = new GamblingRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }


        [Fact]
        public void Evaluate_GamblingRuleTriggeredSomeChecksScoringBelowThreshold_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 200,
                    UniqueForeignMerchantTransactionCountLast24h = 3,
                    ForeignMerchantTransactionsLast24h = new List<(decimal Amount, DateTime Time)> {
                    (11.11m, new DateTime(2026, 8, 10, 14, 30, 10)),
                    (22.22m, new DateTime(2026, 8, 10, 14, 45, 50)),
                    (100m, new DateTime(2026, 8, 10, 14, 57, 30)),
                    (11.77m, new DateTime(2026, 8, 10, 15, 11, 20)),
                    (23.49m, new DateTime(2026, 8, 10, 15, 30, 17))},
                    MerchantCategoryCountLast24h = 1,
                    IncomingFromGamblingMerchantsCountLast24h = 0
                }
                //40
            };
            

            var rule = new GamblingRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_GamblingRuleNotTriggered_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 100,
                    UniqueForeignMerchantTransactionCountLast24h = 2,
                    ForeignMerchantTransactionsLast24h = new List<(decimal Amount, DateTime Time)> {
                    (11.11m, new DateTime(2026, 8, 10, 14, 30, 10))},
                    MerchantCategoryCountLast24h = 1,
                    IncomingFromGamblingMerchantsCountLast24h = 0
                }
                //0
            };
            

            var rule = new GamblingRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_GamblingRuleNotTriggeredNoForeignMerchants_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 100,
                    UniqueForeignMerchantTransactionCountLast24h = 2,
                    ForeignMerchantTransactionsLast24h = new List<(decimal Amount, DateTime Time)> { },
                    MerchantCategoryCountLast24h = 1,
                    IncomingFromGamblingMerchantsCountLast24h = 0
                }
                //0
            };
            

            var rule = new GamblingRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }
    }
}