using Microsoft.EntityFrameworkCore;
using Order.API.Domain.Entities;

namespace Order.API.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Order bounded context.
/// Manages <see cref="Order"/> and <see cref="OrderItem"/> entities.
/// </summary>
public class OrderDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="OrderDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    /// <summary>Gets the DbSet for order records.</summary>
    public DbSet<Domain.Entities.Order> Orders => Set<Domain.Entities.Order>();

    /// <summary>Gets the DbSet for order item records.</summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Status).HasConversion<string>();
            e.Property(o => o.PaymentMethod).HasConversion<string>();
            e.Property(o => o.SubTotal).HasPrecision(10, 2);
            e.Property(o => o.DeliveryFee).HasPrecision(10, 2);
            e.Property(o => o.Tax).HasPrecision(10, 2);
            e.Property(o => o.TotalAmount).HasPrecision(10, 2);
            e.HasMany(o => o.OrderItems)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasPrecision(10, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}