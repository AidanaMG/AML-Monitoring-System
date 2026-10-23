
namespace Aml.Application.RulesAbstractions
{
    public class RuleSettings
    {
        private readonly Dictionary<string, Dictionary<string, decimal>> _ruleSettingsByRuleCode;

        public RuleSettings() => _ruleSettingsByRuleCode = new();
        public RuleSettings(Dictionary<string, Dictionary<string, decimal>> source)
            => _ruleSettingsByRuleCode = source;

        public decimal Get(string ruleCode, string parameter, decimal fallback)
        {
            if(_ruleSettingsByRuleCode.TryGetValue(ruleCode, out var parameters)
                && parameters.TryGetValue(parameter, out var value))
                return value;

            return fallback;
        }

        public decimal GetWeight(string ruleCode, decimal fallback)
            => Get(ruleCode, "__weight", fallback); 
    }
}
