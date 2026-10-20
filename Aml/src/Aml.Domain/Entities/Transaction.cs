using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public TransactionDirection Direction { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountKzt { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public int? TransactionCountryId { get; set; }
        
        public int? FromAccountId { get; set; }
        public string? FromAccountNumber { get; set; }
        public string? FromCardNumber { get; set; }    
        
        public int? ToAccountId { get; set; }
        public string? ToAccountNumber { get; set; }
        public string? ToCardNumber { get; set; }      

        public string? CounterpartyBank { get; set; } = null!;
        public int? CounterpartyCountryId { get; set; }

        public int? MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public string? CounterpartyName { get; set; }

        public bool IsFlagged { get; set; }
    }
}
