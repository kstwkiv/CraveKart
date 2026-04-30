using Microsoft.EntityFrameworkCore;
using Payment.API.Domain.Entities;

namespace Payment.API.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Payment bounded context.
/// Manages <see cref="Payment"/> entities.
/// </summary>
public class PaymentDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="PaymentDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    /// <summary>Gets the DbSet for payment records.</summary>
    public DbSet<Payment.API.Domain.Entities.Payment> Payments => Set<Payment.API.Domain.Entities.Payment>();

    /// <inheritdoc/>
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