using Microsoft.EntityFrameworkCore;
using TimeOffAccrual.Api.Models;

namespace TimeOffAccrual.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<CreditBalance> CreditBalances => Set<CreditBalance>();
    public DbSet<TimeOffRequest> TimeOffRequests => Set<TimeOffRequest>();
    public DbSet<CreditAdjustment> CreditAdjustments => Set<CreditAdjustment>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<CreditBalance>(e =>
        {
            e.Ignore(c => c.AvailableHours); // getter only, no setter, column not created
            e.HasIndex(c => c.UserId).IsUnique(); // one record per user only
            e.Property(c => c.ConcurrencyToken).IsConcurrencyToken();

            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);


        });

        modelBuilder.Entity<TimeOffRequest>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20); // enums stored as text
            e.Property(r => r.DurationType).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.ConcurrencyToken).IsConcurrencyToken();

            e.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(r => new { r.UserId, r.StartDate, r.EndDate }); // overlap query
            e.HasIndex(r => r.Status); // approval queue

            e.ToTable(t => t.HasCheckConstraint("CK_TimeOffRequest_Dates", "EndDate >= StartDate"));

            e.HasOne(r => r.DecidedByUser)
                .WithMany()
                .HasForeignKey(r => r.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CreditAdjustment>(e =>
        {
            e.HasOne(a => a.AgentUser)
                .WithMany()
                .HasForeignKey(a => a.AgentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.AdminUser)
                .WithMany()
                .HasForeignKey(a => a.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => a.AgentUserId);
        });
    }
}