using Payment.API.Application.Interfaces;
using Payment.API.Infrastructure.Persistence;

namespace Payment.API.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;

    public UnitOfWork(PaymentDbContext context)
    {
        _context = context;
        Payments = new PaymentRepository(context);
    }

    public IPaymentRepository Payments { get; }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}