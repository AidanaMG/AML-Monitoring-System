using Aml.Domain.Enums;

namespace Aml.Domain.Entities
{

    public class KnownCounterparty
    {
        public int Id { get; set; }
        public string Name { get; private set; } = null!;
        public string NormalizedName { get; private set; } = null!; 
        public CounterpartyType Type { get; private set; }
        public string? Source { get; private set; }
        public DateTime AddedDate { get; private set; }
        public DateTime? RemovedDate { get; private set; }

        public bool IsActive => RemovedDate is null;


        private KnownCounterparty() { } 

        public static KnownCounterparty Create(string name, CounterpartyType type, string? source, DateTime addedDate)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Counterparty name is required", nameof(name));
            }

            return new() {
                Name = CounterpartyName.CleanWhitespace(name),
                NormalizedName = CounterpartyName.Normalize(name),
                Type = type,
                Source = source,
                AddedDate = addedDate};
        }

        public void Deactivate(DateTime removedDate)
        {
            if(RemovedDate is not null)
            {
                throw new InvalidOperationException("Unable to deactivate: removal date is already set");
            }
            
            if(removedDate < AddedDate)
            {
                throw new InvalidOperationException("Unable to deactivate: removal date is earlier than addition date");
            }

            RemovedDate = removedDate;
        }


    }
}

