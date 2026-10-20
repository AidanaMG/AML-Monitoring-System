namespace Aml.Application.RulesAbstractions
{
    public record RuleResult(
        bool Triggered,     
        int MatchedCount,   
        int TotalChecks,    
        int Contribution,   
        string? TriggerDetails
    );
}
