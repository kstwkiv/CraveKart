using Microsoft.EntityFrameworkCore;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment.API.Domain.Entities.Payment> Payments => Set<Payment.API.Domain.Entities.Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment.API.Domain.Entities.Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasPrecision(10, 2);
            e.Property(p => p.Status).HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}