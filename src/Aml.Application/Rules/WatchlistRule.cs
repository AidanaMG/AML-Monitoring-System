using Aml.Application.RulesAbstractions;
using Aml.Domain.Enums;

namespace Aml.Application.Rules

{
    public class WatchlistRule : ITransactionRule
    {
        public string Code => "WATCHLIST";
        public RuleType Type => RuleType.Hard;

        
       
        public RuleResult Evaluate(RuleContext context)
        {
            bool isTriggered = context.ClientFacts.ActiveWatchlistEntries.Count > 0;
            int recordCounts = context.ClientFacts.ActiveWatchlistEntries.Count;
            string detailsInfo = "";

            string details = string.Join(", ", context.ClientFacts.ActiveWatchlistEntries.Select(l => l.ListType));

            if (isTriggered)
            {
                detailsInfo = $"Client was found on the following lists: {details}";
            }
            else
            {
                detailsInfo = "Triggered checks: none";
            }

            // The concept of total checks is not applicable, just a record counter
            return new RuleResult(isTriggered, recordCounts, recordCounts, 0, detailsInfo); 

        }
    }
}
