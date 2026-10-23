
using Aml.Domain.Enums;

namespace Aml.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
