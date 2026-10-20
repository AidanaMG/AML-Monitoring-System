using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class DrugRuleTests
    {
        [Fact]
        public void Evaluate_DrugRuleTriggeredAllChecks_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAge = 20,
                    ClientAgeDays = 50
                },

                Accounts = new List<AccountActivity>
                { 
                    new() { AccountId = 1,  CurrentAccountBalance = 100m, IncomingP2pAmountSumLast24h = 500_000m}
                },

                ClientActivity = new ClientActivity
                {
                    IncomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10)),
                    (16100m, new DateTime(2026, 8, 11, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 12, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 13, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 14, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 15, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 16, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 17, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 18, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 18, 16, 37, 23))},
                    OutcomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10)),
                    (16100m, new DateTime(2026, 8, 11, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 12, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 13, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 14, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 15, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 16, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 17, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 18, 17, 37, 23)),
                    (16100m, new DateTime(2026, 8, 18, 16, 37, 23))},
                    UniqueIncomingCardCountLast24h = 11,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m },
                    UniqueIncomingCounterpartiesLast24h = 10,
                    MinutesToFirstWithdrawalAfterIncoming = 3
                }
                //100
            };

            var rule = new DrugRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);
        }



        [Fact]
        public void Evaluate_DrugRuleTriggeredSomeChecksScoringAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAge = 26,
                    ClientAgeDays = 500
                },

                Accounts = new List<AccountActivity>
                { 
                    new() { AccountId = 1,  CurrentAccountBalance = 100m, IncomingP2pAmountSumLast24h = 500_000m}
                },

                ClientActivity = new ClientActivity
                {
                    IncomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10))},
                    OutcomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10)) },
                    UniqueIncomingCardCountLast24h = 11,
                    IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m },
                    UniqueIncomingCounterpartiesLast24h = 10,
                    MinutesToFirstWithdrawalAfterIncoming = 14
                }
                //75
            };

            var rule = new DrugRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered);
        }



        [Fact]
        public void Evaluate_DrugRuleTriggeredSomeChecksScoringBelowThreshold_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAge = 25,
                    ClientAgeDays = 89
                },

               Accounts = new List<AccountActivity>
               { 
                new() { AccountId = 1, CurrentAccountBalance = 100m, IncomingP2pAmountSumLast24h = 500_000m}

               },

               ClientActivity = new ClientActivity
               {
                   IncomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10))},
                   OutcomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> {
                    (15700m, new DateTime(2026, 8, 10, 14, 30, 10)) },
                   UniqueIncomingCardCountLast24h = 3,
                   IncomingP2pAmountsLast24h = new List<decimal> { 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m, 50_000m },
                   UniqueIncomingCounterpartiesLast24h = 4,
                   MinutesToFirstWithdrawalAfterIncoming = 10
               }
                //65
            };

            var rule = new DrugRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }



        [Fact]
        public void Evaluate_DrugRuleNotTriggered_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ClientAge = 29,
                    ClientAgeDays = 125
                },
                
                Accounts = new List<AccountActivity>
                {
                    new() { AccountId = 1, CurrentAccountBalance = 100_000m, IncomingP2pAmountSumLast24h = 0}
                },

                ClientActivity = new ClientActivity
                {
                    IncomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> { },
                    OutcomingP2pTransactionsLast3Days = new List<(decimal Amount, DateTime Time)> { },
                    UniqueIncomingCardCountLast24h = 0,
                    IncomingP2pAmountsLast24h = new List<decimal> { },
                    UniqueIncomingCounterpartiesLast24h = 0,
                    MinutesToFirstWithdrawalAfterIncoming = null
                }
                //0
            };

            var rule = new DrugRule();
            var result = rule.Evaluate(context);

            Assert.False(result.Triggered);
        }
    }
}

