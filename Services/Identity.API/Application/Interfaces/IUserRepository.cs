using Identity.API.Domain.Entities;

namespace Identity.API.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    void Update(User user);
}