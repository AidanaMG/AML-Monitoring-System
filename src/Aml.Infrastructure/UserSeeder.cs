using Aml.Domain.Entities;
using Aml.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aml.Infrastructure
{
    public static class UserSeeder
    {
        private static readonly DateTime SeedDate = new(2026, 1, 1);

        public static async Task SeedAsync(AmlDbContext db, CancellationToken ct)
        {
            if(await db.Users.AnyAsync(ct)) return;

            var hasher = new PasswordHasher<User>();

            var officer = new User
            {
                UserName = "ComplianceOfficer",
                Role = UserRole.Officer,
                IsActive = true,
                CreatedAt = SeedDate
            };

            officer.PasswordHash = hasher.HashPassword(officer, "Officer123!");


            var admin = new User
            {
                UserName = "Administrator",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt= SeedDate
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");


            db.Users.AddRange(officer, admin);
            await db.SaveChangesAsync(ct);
        }
    }
}
