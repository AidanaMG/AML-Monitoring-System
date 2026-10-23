using Aml.Domain.Enums;


namespace Aml.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }  = null!;
        public string Surname { get; set; } = null!;
        public string? Patronymic { get; set; }
        public string Iin { get; set; } = null!;
        public ClientStatus Status { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Citizenship { get; set; } = null!;
        public int CountryId { get; set; }
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public RiskLevel RiskLevel { get; set; }
        public DateTime JoinDate { get; set; }
        public PepStatus PepStatus { get; set; }
        public string? PepPosition { get; set; }
        public DateTime? PepSince { get; set; }
        public bool IsFlagged { get; set; }
        public decimal ScoringValue { get; set; }
        public string? ScoringRole { get; set; }
    }
}
