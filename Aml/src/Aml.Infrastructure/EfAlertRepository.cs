using Aml.Application.Screening;
using Aml.Domain.Entities;

namespace Aml.Infrastructure
{
    public class EfAlertRepository : IAlertRepository
    {
        private readonly AmlDbContext _db;
        public EfAlertRepository(AmlDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Alert alert, CancellationToken ct)
        {
            _db.Alerts.Add(alert);
            await _db.SaveChangesAsync(ct);
        }

    }
}
