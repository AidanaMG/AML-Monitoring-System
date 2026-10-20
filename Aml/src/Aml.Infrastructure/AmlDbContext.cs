using Microsoft.EntityFrameworkCore;
using Aml.Domain.Entities;

namespace Aml.Infrastructure
{
    public class AmlDbContext : DbContext
    {
        public AmlDbContext(DbContextOptions<AmlDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<MccCode> MccCodes => Set<MccCode>();
        public DbSet<WatchList> WatchLists => Set<WatchList>();
        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<Rule> Rules => Set<Rule>();
        public DbSet<ExchangeRates> ExchangeRates => Set<ExchangeRates>();
        public DbSet<KnownCounterparty> KnownCounterparties => Set<KnownCounterparty>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KnownCounterparty>(entity =>
            {
                entity.Ignore(c => c.IsActive);

                entity.Property(c => c.Name).IsRequired().HasMaxLength(300);
                entity.Property(c => c.NormalizedName).IsRequired().HasMaxLength(300);
                entity.Property(c => c.Source).HasMaxLength(200);

                entity.HasIndex(c => new { c.NormalizedName, c.Type })
                    .IsUnique()
                    .HasFilter("[RemovedDate] IS NULL");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasIndex(t => new { t.FromAccountId, t.TransactionDate});
                entity.HasIndex(t => new { t.ToAccountId, t.TransactionDate});

                entity.Property(t => t.Amount).HasPrecision(11, 2);
                entity.Property(t => t.AmountKzt).HasPrecision(11, 2);                

                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(t => t.FromAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(t => t.ToAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Country>()
                    .WithMany()
                    .HasForeignKey(t => t.TransactionCountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Country>()
                    .WithMany()
                    .HasForeignKey(t => t.CounterpartyCountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Merchant>()
                    .WithMany()
                    .HasForeignKey(t => t.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.Balance).HasPrecision(11, 2);

                entity.HasOne<Client>()
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasOne<MccCode>()
                    .WithMany()
                    .HasForeignKey(m => m.MccId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Country>()
                    .WithMany()
                    .HasForeignKey(m => m.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(m => m.Name).IsUnique();
            });

            modelBuilder.Entity<WatchList>(entity =>
            {
                entity.HasOne<Client>()
                    .WithMany()
                    .HasForeignKey(w => w.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.Property(a => a.RiskScore).HasPrecision(3, 0);

                entity.HasOne<Client>()
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Transaction>()
                    .WithMany()
                    .HasForeignKey(a => a.TransactionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.ResolutionComment)
                    .HasMaxLength(500);

                entity.Property(a => a.AssignedTo)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.ScoringValue).HasPrecision(3, 0);

                entity.HasOne<Country>()
                    .WithMany()
                    .HasForeignKey(c => c.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rule>(entity =>
            {
                entity.Property(r => r.Weight).HasPrecision(3, 0);
                entity.Property(r => r.RuleCode).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.RuleCode).IsUnique();

            });

            modelBuilder.Entity<ExchangeRates>(entity =>
            {
                entity.Property(e => e.RateToKzt).HasPrecision(11, 6);

            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.Property(c => c.Code)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.HasIndex(c => c.Code).IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(u => u.UserName).IsUnique();

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(200);
            });


            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(l => l.UserName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(l => l.EntityType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(l => l.OldValue)
                    .HasMaxLength(1000);

                entity.Property(l => l.NewValue)
                    .HasMaxLength(1000);

                entity.HasIndex(l => l.CreatedAt);
                entity.HasIndex(l => new { l.EntityType, l.EntityId});
            });
        }

    }
}