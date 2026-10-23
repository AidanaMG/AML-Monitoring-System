using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public AccountType Type { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? CardNumber { get; set; }
    }
}
