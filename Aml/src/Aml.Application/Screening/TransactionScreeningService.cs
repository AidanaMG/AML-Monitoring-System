using Aml.Application.RulesAbstractions;
using Aml.Domain.Enums;


namespace Aml.Application.Screening
{
    /*
    * Scoring rules fall into three categories 
    * 
    * 1. Typology rules (Dropper/Drug/Crypto/Gambling): each rule accumulates its own
    *    internal contribution and only counts as triggered once that contribution
    *    reaches TypologyConfidenceScoreThreshold - i.e. it matched most of the
    *    behavioral pattern, not just a few isolated checks. This prevents blocking
    *    a client who only triggers a few unrelated checks across different, unconnected
    *    typologies
    *    
    * 2. Risk-factor rules (HighFrequency, HighRiskMerchant, HighRiskCountry): individually 
    *    weak signals, none of which alone reaches ScoringScoreThreshold. Meant to accumulate 
    *    together toward it. Room for extension: new signals that matter, but don't belong
    *    to a single dedicated pattern can be added.
    *    
    * 3. Hard-rules (CriticalRiskCountry, CriticalRiskMerchant, SanctionList): triggers on a
    *    single match - the signal alone is certain enough to block the client and transaction.
    */

    public class TransactionScreeningService
    {
        private readonly IEnumerable<ITransactionRule> _rules;

        public TransactionScreeningService(IEnumerable<ITransactionRule> rules)
        {
            _rules = rules;
            
        }


        public ScreeningResult Screening(RuleContext context)
        {
            List<RuleEvaluation> ruleEvaluations = new List<RuleEvaluation>();
            decimal totalScore = 0;

            bool hardTriggered = false; 
            bool typologyTriggered = false; 


            foreach (var rule in _rules)
            {
                bool activeRuleFilterEnabled = context.ActiveRuleCodes.Count > 0;

                if(activeRuleFilterEnabled && !context.ActiveRuleCodes.Contains(rule.Code)) continue;

                var result = rule.Evaluate(context);

                if (result.Triggered)
                {
                    if (rule.Type == RuleType.Hard) hardTriggered = true;
                    if (rule.Type == RuleType.Typology) typologyTriggered = true;
                }

                if (rule.Type == RuleType.Scoring && result.Triggered)
                {
                    totalScore += result.Contribution;
                }

                ruleEvaluations.Add(new RuleEvaluation(rule.Code, result, rule.Type));


            }


            bool shouldBlock = hardTriggered || typologyTriggered || totalScore >= ScreeningThresholds.ScoringScoreThreshold;

            return new ScreeningResult(shouldBlock, totalScore, ruleEvaluations, hardTriggered, typologyTriggered);
        }



    }
}
