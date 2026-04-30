using Identity.API.Application.Interfaces;
using Identity.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/> for managing user account persistence.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    /// <inheritdoc/>
    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);

    /// <inheritdoc/>
    public async Task AddAsync(User user) =>
        await _context.Users.AddAsync(user);

    /// <inheritdoc/>
    public void Update(User user) =>
        _context.Users.Update(user);
}