using Microsoft.EntityFrameworkCore;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Restaurant bounded context.
/// Manages <see cref="Restaurant"/>, <see cref="MenuCategory"/>, <see cref="MenuItem"/>, and <see cref="Review"/> entities.
/// </summary>
public class RestaurantDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="RestaurantDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options) { }

    /// <summary>Gets the DbSet for restaurant records.</summary>
    public DbSet<Domain.Entities.Restaurant> Restaurants => Set<Domain.Entities.Restaurant>();

    /// <summary>Gets the DbSet for menu category records.</summary>
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();

    /// <summary>Gets the DbSet for menu item records.</summary>
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    /// <summary>Gets the DbSet for customer review records.</summary>
    public DbSet<Review> Reviews => Set<Review>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Restaurant>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(200);
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.AverageRating).HasPrecision(3, 2);
            e.Property(r => r.MinimumOrderAmount).HasPrecision(10, 2);
        });

        modelBuilder.Entity<MenuCategory>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.Restaurant)
             .WithMany(r => r.MenuCategories)
             .HasForeignKey(c => c.RestaurantId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Price).HasPrecision(10, 2);
            e.HasOne(i => i.Category)
             .WithMany(c => c.MenuItems)
             .HasForeignKey(i => i.CategoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.ReviewText).HasMaxLength(500);
            e.HasOne(r => r.Restaurant)
             .WithMany()
             .HasForeignKey(r => r.RestaurantId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.OrderId, r.CustomerId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}