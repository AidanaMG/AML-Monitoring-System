using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class WatchList
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public ListType ListType { get; set; }
        public string? Source { get; set; }
        public string? ClientStatus { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; } = null!;
        public DateTime? RemovedDate { get; set; }
        public string? RemovedBy { get; set; }
    }
}
