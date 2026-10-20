using Aml.Domain.Enums;


namespace Aml.Application.RulesAbstractions
{
    public interface ITransactionRule
    {
        string Code { get; }        
        RuleType Type { get; }
        RuleResult Evaluate(RuleContext context);
    }
}
