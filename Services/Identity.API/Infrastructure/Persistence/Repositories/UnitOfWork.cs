using Identity.API.Application.Interfaces;

namespace Identity.API.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _context;

    public UnitOfWork(IdentityDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
    }

    public IUserRepository Users { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}