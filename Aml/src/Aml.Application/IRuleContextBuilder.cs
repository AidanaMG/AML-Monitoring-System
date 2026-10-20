using Aml.Application.RulesAbstractions;
using Aml.Domain.Entities;

namespace Aml.Application
{
    public interface IRuleContextBuilder
    {
        Task<RuleContext> BuildAsync(Transaction transaction, DateTime evaluationTime, CancellationToken ct);
    }
}
