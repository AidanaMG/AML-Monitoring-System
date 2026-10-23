using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class DropperRuleTests
    {

        [Fact]
        public void Evaluate_DropperRuleTriggeredAllChecks_ReturnsTriggered() 
        {
            var context = new RuleContext   
            {
                ClientFacts = new ClientFacts
                {
                    ClientAgeDays = 70
                },

                Accounts = new List<AccountActivity>
                {
                    new() { AccountId = 1, CurrentAccountBalance = 200m, IncomingP2pAmountSumLast24h = 3_000_040m}
                },

                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 10,
                    MinutesToFirstWithdrawalAfterIncoming = 5,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 100_000m, 100_000m, 20_000m, 200_000m, 250_000m, 300_000m, 500_000m, 50_000m, 100_000m, 100_000m, 20_000m, 200_000m, 250_000m, 300_000m, 500_000m },
                    OutcomingP2pAmountsLast24h = new List<decimal> { 100_000m, 100_000m, 20_000m, 200_000m, 250_000m, 300_000m, 500_000m, 200_000m, 250_000m, 300_000m, 500_000m },
                    UniqueIncomingCounterpartiesLast24h = 11,
                    UniqueOutcomingCounterpartiesLast24h = 7,
                    CounterpartiesInAntifraudListCount = 1
                }
                //100
            };

            
            var rule = new DropperRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }



        [Fact]
        public void Evaluate_DropperRuleTriggeredSomeChecksScoringAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext   
            {
                ClientFacts = new ClientFacts
                {
                    ClientAgeDays = 50
                },

                Accounts = new List<AccountActivity>
                {
                    new() { AccountId = 1, CurrentAccountBalance = 150_000m, IncomingP2pAmountSumLast24h = 2_000_000m}
                },

                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 100,
                    MinutesToFirstWithdrawalAfterIncoming = 10,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 150_000m, 150_000m, 150_000m },
                    OutcomingP2pAmountsLast24h = new List<decimal> { 50_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 150_000m, 150_000m, 150_000m },
                    UniqueIncomingCounterpartiesLast24h = 5,
                    UniqueOutcomingCounterpartiesLast24h = 0,
                    CounterpartiesInAntifraudListCount = 0
                }
                //82
            }; 


            var rule = new DropperRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }



        [Fact]
        public void Evaluate_DropperRuleNotTriggered_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAgeDays = 200
                },

                Accounts = new List<AccountActivity>
                {
                    new() { AccountId = 1, CurrentAccountBalance = 500_000m, IncomingP2pAmountSumLast24h = 245_000m}
                },

                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 150,
                    MinutesToFirstWithdrawalAfterIncoming = 120,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 175_000m, 20_000m },
                    OutcomingP2pAmountsLast24h = new List<decimal> { 20_500m },
                    UniqueIncomingCounterpartiesLast24h = 2,
                    UniqueOutcomingCounterpartiesLast24h = 0,
                    CounterpartiesInAntifraudListCount = 0
                }
                //0
            };


            var rule = new DropperRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }



        [Fact]
        public void Evaluate_DropperRuleTriggeredSomeChecksScoringBelowThreshold_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAgeDays = 50
                },

                Accounts = new List<AccountActivity>
                {
                    new() { AccountId = 1, CurrentAccountBalance = 150_000m, IncomingP2pAmountSumLast24h = 2_000_000m}
                },

                ClientActivity = new ClientActivity
                {
                    RetailTransactionCountLast30Days = 100,
                    MinutesToFirstWithdrawalAfterIncoming = 30,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 150_000m, 150_000m, 150_000m },
                    OutcomingP2pAmountsLast24h = new List<decimal> { 50_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 250_000m, 150_000m, 150_000m, 150_000m },
                    UniqueIncomingCounterpartiesLast24h = 5,
                    UniqueOutcomingCounterpartiesLast24h = 0,
                    CounterpartiesInAntifraudListCount = 0
                }
                //67
            };


            var rule = new DropperRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }

    }
}

