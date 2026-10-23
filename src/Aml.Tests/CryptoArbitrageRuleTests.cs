using Aml.Application.Rules;
using Aml.Application.RulesAbstractions;

namespace Aml.Tests
{
    public class CryptoArbitrageRuleTests
    {

        [Fact]
        public void Evaluate_CryptoRuleTriggeredAllChecks_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    P2pAndIntrabankAmountsLast30Days = new List<decimal> { 329_897m, 40_978m, 78_367m, 329_897m, 40_978m, 78_367m, 1_874m, 582m, 8_542m, 9_898m, 329_897m, 40_978m, 78_367m, 329_897m, 40_978m, 329_897m, 40_978m, 78_367m, 329_897m, 40_978m, 78_367m, 1_874m, 582m, 8_542m, 9_898m },
                    P2pAndIntrabankTransactionCountLast30Days = 25,
                    UniqueCounterpartyCountLast30Days = 25,
                    NonResidentCounterpartyCountLast30Days = 5,
                    NonAifcCryptoExchangeTransactionCountLast30Days = 3
                }
                //100
            };


            var rule = new CryptoArbitrageRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }



        [Fact]
        public void Evaluate_CryptoRuleTriggeredSomeChecksScoringAboveThreshold_ReturnsTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    P2pAndIntrabankAmountsLast30Days = new List<decimal> { 329_897m, 40_978m, 78_367m, 40_978m, 78_000m },
                    P2pAndIntrabankTransactionCountLast30Days = 5,
                    UniqueCounterpartyCountLast30Days = 26,
                    NonResidentCounterpartyCountLast30Days = 5,
                    NonAifcCryptoExchangeTransactionCountLast30Days = 1
                }
                //80
            };


            var rule = new CryptoArbitrageRule();
            var result = rule.Evaluate(context);


            Assert.True(result.Triggered);
        }


        [Fact]
        public void Evaluate_CryptoRuleTriggeredSomeChecksScoringBelowThreshold_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    P2pAndIntrabankAmountsLast30Days = new List<decimal> { 329_897m, 40_978m, 78_367m, 100_055m, 10_000m, 1_050 },
                    P2pAndIntrabankTransactionCountLast30Days = 6,
                    UniqueCounterpartyCountLast30Days = 1,
                    NonResidentCounterpartyCountLast30Days = 5,
                    NonAifcCryptoExchangeTransactionCountLast30Days = 2
                }
                //60
            };


            var rule = new CryptoArbitrageRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }



        [Fact]
        public void Evaluate_CryptoRuleNotTriggered_ReturnsNotTriggered() 
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    P2pAndIntrabankAmountsLast30Days = new List<decimal> { 10_000m, 25_090m },
                    P2pAndIntrabankTransactionCountLast30Days = 2,
                    UniqueCounterpartyCountLast30Days = 5,
                    NonResidentCounterpartyCountLast30Days = 0,
                    NonAifcCryptoExchangeTransactionCountLast30Days = 0
                }
                //0
            };


            var rule = new CryptoArbitrageRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }


        [Fact]
        public void Evaluate_CryptoRuleNotTriggeredWithNoTransfers_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientActivity = new ClientActivity
                {
                    P2pAndIntrabankAmountsLast30Days = new List<decimal> { },
                    P2pAndIntrabankTransactionCountLast30Days = 0,
                    UniqueCounterpartyCountLast30Days = 0,
                    NonResidentCounterpartyCountLast30Days = 0,
                    NonAifcCryptoExchangeTransactionCountLast30Days = 0
                }
                //0
            };


            var rule = new CryptoArbitrageRule();
            var result = rule.Evaluate(context);


            Assert.False(result.Triggered);
        }



    }
}

