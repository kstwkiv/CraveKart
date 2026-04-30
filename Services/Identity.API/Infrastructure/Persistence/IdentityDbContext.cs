using Identity.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Identity bounded context.
/// Manages <see cref="User"/> entities with unique email constraints.
/// </summary>
public class IdentityDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityDbContext"/> with the specified options.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    /// <summary>Gets the DbSet for user accounts.</summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.MobileNumber).HasMaxLength(20);
            entity.Property(u => u.Role).HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}