
namespace Aml.Domain.Entities
{
    public class Merchant
    {
        public int Id { get; set; }
        public int MccId { get; set; }

        public string Name { get; set; } = null!;
        public bool? IsAifc { get; set; }
        public int? CountryId { get; set; } 

    }
}
