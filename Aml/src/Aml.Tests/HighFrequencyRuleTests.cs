using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;
using Aml.Application.Screening;

namespace Aml.Tests
{
    public class HighFrequencyRuleTests
    {
        [Fact]
        public void Evaluate_HighFrequencyDetected_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    TransactionCountLastHour = CriticalAndHighRiskThresholds.TransactionCountLastHourMinRange
                }
                
            };

            var rule = new HighFrequencyRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);
        }


        [Fact]
        public void Evaluate_NoHighFrequencyDetected_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    TransactionCountLastHour = 10
                }
                
            };

            var rule = new HighFrequencyRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_NoTransactionsLastHourDetected_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    TransactionCountLastHour = 0
                }
                
            };

            var rule = new HighFrequencyRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }
    }
}
