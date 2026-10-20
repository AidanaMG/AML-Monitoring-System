using Aml.Domain.Entities;

namespace Aml.Simulator.ProfilesAdditions
{
    public record GeneratedProfile(Client Client, 
                                   Account Account, 
                                   List<Transaction> Transactions, 
                                   List<WatchList> WatchlistEntries);
}
