using Delivery.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Delivery.API.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Delivery bounded context.
/// Manages <see cref="Delivery"/> and <see cref="DeliveryAgent"/> entities.
/// </summary>
public class DeliveryDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="DeliveryDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

    /// <summary>Gets the DbSet for delivery records.</summary>
    public DbSet<Domain.Entities.Delivery> Deliveries => Set<Domain.Entities.Delivery>();

    /// <summary>Gets the DbSet for delivery agent profiles.</summary>
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Delivery>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Agent)
             .WithMany()
             .HasForeignKey(d => d.AgentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeliveryAgent>(e =>
        {
            e.HasKey(a => a.Id);
        });

        base.OnModelCreating(modelBuilder);
    }
}