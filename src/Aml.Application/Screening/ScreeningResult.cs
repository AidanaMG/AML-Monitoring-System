using Aml.Application.RulesAbstractions;
using Aml.Domain.Enums;

namespace Aml.Application.Screening
{
    public record ScreeningResult(
        bool ShouldBlock,
        decimal TotalScore,
        IReadOnlyList<RuleEvaluation> RuleEvaluations,
        bool HardTriggered,
        bool TypologyTriggered
    );

    public record RuleEvaluation(string RuleCode, RuleResult Result, RuleType RuleType);
}
