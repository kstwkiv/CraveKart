using Delivery.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Delivery.API.Infrastructure.Persistence;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Delivery> Deliveries => Set<Domain.Entities.Delivery>();
    public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();

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