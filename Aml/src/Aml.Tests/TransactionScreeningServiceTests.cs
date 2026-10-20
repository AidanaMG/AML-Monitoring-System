using Aml.Domain.Enums;
using Aml.Application.Screening;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class TransactionScreeningServiceTests
    {
        [Fact]
        public void Screening_HardFakeRule_ReturnsTriggered()
        {
            var context = new RuleContext();

            var fakeRules = new List<ITransactionRule> {
                new FakeRule("FAKE_HARD", RuleType.Hard, new RuleResult(true, 1, 1, 0, "hard fake rule test true")) };
                
                
            var service = new TransactionScreeningService(fakeRules);
            var result = service.Screening(context);

            Assert.True(result.ShouldBlock);
        }

        [Fact]
        public void Screening_ScoringFakeRuleAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext();

            var fakeRules = new List<ITransactionRule> {
                new FakeRule("FAKE_SCORING", RuleType.Scoring, new RuleResult(true, 2, 2, ScreeningThresholds.ScoringScoreThreshold, "scoring fake rule test true"))};
       
            var service = new TransactionScreeningService(fakeRules);
            var result = service.Screening(context);

            Assert.True(result.ShouldBlock);
        
        }

        [Fact]
        public void Screening_ScoringFakeRuleBelowThreshold_ReturnsNotTriggered()
        {
            var context = new RuleContext();

            var fakeRules = new List<ITransactionRule> { 
                new FakeRule("FAKE_SCORING", RuleType.Scoring, new RuleResult(true, 1, 3, 20, "scoring fake rule test false"))};

            var service = new TransactionScreeningService(fakeRules);
            var result = service.Screening(context);

            Assert.False(result.ShouldBlock);
        
        }

        
        [Fact]
        public void Screening_NoFakeRuleTriggered_ReturnsNotTriggered()
        {
            var context = new RuleContext();

            var fakeRules = new List<ITransactionRule> {
                new FakeRule("NO_RULE_TRIGGERED", RuleType.Scoring, new RuleResult(false, 1, 1, ScreeningThresholds.ScoringScoreThreshold, "no rule triggered test"))
            };

            var service = new TransactionScreeningService(fakeRules);
            var result = service.Screening(context);

            Assert.False(result.ShouldBlock);
        }


        [Fact]
        public void Screening_TwoScoringFakeRulesCombinedAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext();
            var fakeRules = new List<ITransactionRule> {
                new FakeRule("Fake_A", RuleType.Scoring, new RuleResult(true, 1, 1, CriticalAndHighRiskThresholds.HighFrequencyScoringScoreThreshold, "fake a")),
                new FakeRule("Fake_B", RuleType.Scoring, new RuleResult(true, 1, 1, CriticalAndHighRiskThresholds.HighRiskMerchantScoringScoreThreshold, "fake b"))
            };

            var service = new TransactionScreeningService(fakeRules);
            var result = service.Screening(context);

            Assert.True(result.ShouldBlock);
        }


    }
}
