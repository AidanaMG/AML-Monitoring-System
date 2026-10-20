
namespace Aml.Domain.Entities
{
    public class ExchangeRates
    {
        public int Id { get; set; }
        public string Currency { get; set; } = null!;
        public decimal RateToKzt { get; set; }
        public DateTime RateDate { get; set; }
    }
}
