using Payment.API.Application.Interfaces;
using Payment.API.Infrastructure.Persistence;

namespace Payment.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> for the Payment bounded context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWork"/> and creates the payment repository.
    /// </summary>
    /// <param name="context">The shared database context.</param>
    public UnitOfWork(PaymentDbContext context)
    {
        _context = context;
        Payments = new PaymentRepository(context);
    }

    /// <inheritdoc/>
    public IPaymentRepository Payments { get; }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}