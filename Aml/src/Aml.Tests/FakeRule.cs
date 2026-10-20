using Aml.Application.RulesAbstractions;
using Aml.Domain.Enums;

namespace Aml.Tests
{
    public class FakeRule : ITransactionRule
    {
        private readonly RuleResult _ruleResult;

        public string Code { get; }
        public RuleType Type { get; }
        public RuleResult Evaluate(RuleContext context) => _ruleResult;


        public FakeRule(string code, RuleType type, RuleResult result)
        {
            Code = code;
            Type = type;
            _ruleResult = result;
        }

       
    }
}
