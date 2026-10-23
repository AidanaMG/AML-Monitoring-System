using Aml.Application.Rules;
using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Aml.Application.RulesAbstractions;


namespace Aml.Tests
{
    public class WatchlistRuleTests
    {
        [Fact]
        public void Evaluate_ClientInWatchlist_ReturnsTriggered()
        {
            var context = new RuleContext { 
                ClientFacts = new ClientFacts
                {
                    ActiveWatchlistEntries = new List<WatchList> { new WatchList { ListType = ListType.BankBlacklist } }
                }
                
            
            };

            var rule = new WatchlistRule();
            var result = rule.Evaluate(context);

            Assert.True(result.Triggered); 
        }

        [Fact]
        public void Evaluate_ClientNotInAnyList_ReturnsNotTriggered()
        {
            var context = new RuleContext
            {
                ClientFacts = new ClientFacts
                {
                    ActiveWatchlistEntries = new List<WatchList> { }
                }
                

            };

            var rule = new WatchlistRule();
            var result = rule.Evaluate(context);
            
            Assert.False(result.Triggered);
        }
    }
}
