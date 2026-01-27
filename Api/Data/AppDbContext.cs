using Microsoft.EntityFrameworkCore;
using Api.Entities;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // User
        // =========================
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        });

        // =========================
        // RefreshToken
        // =========================
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.TokenHash).IsUnique();

            e.HasOne(x => x.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // Transaction
        // =========================
        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Amount)
             .HasPrecision(18, 2)
             .IsRequired();

            e.Property(x => x.Description)
             .HasMaxLength(255)
             .IsRequired();

            e.Property(x => x.Type)
             .IsRequired();

            e.Property(x => x.Timing)
             .IsRequired();

            e.Property(x => x.StartDate)
             .IsRequired();

            e.HasIndex(x => new { x.UserId, x.StartDate });
            e.HasIndex(x => x.Type);
            e.HasIndex(x => x.Timing);

            e.HasOne(x => x.User)
             .WithMany(u => u.Transactions)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
