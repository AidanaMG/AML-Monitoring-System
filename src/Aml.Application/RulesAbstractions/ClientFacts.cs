using Aml.Domain.Entities;

namespace Aml.Application.RulesAbstractions
{
    public class ClientFacts
    {
        public IReadOnlyList<WatchList> ActiveWatchlistEntries { get; init; } = new List<WatchList>();
        public int ClientAgeDays { get; init; } 
        public int ClientAge { get; init; }

        public int ClientId { get; init; }

    }
}
