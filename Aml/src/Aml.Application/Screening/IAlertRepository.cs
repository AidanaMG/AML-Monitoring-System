using Aml.Domain.Entities;

namespace Aml.Application.Screening
{
    public interface IAlertRepository
    {
        Task AddAsync(Alert alert, CancellationToken ct);
    }
}
