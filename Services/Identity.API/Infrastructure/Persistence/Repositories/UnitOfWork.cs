using Identity.API.Application.Interfaces;

namespace Identity.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Identity bounded context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates the user repository.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(IdentityDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
    }

    /// <inheritdoc/>
    public IUserRepository Users { get; }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}