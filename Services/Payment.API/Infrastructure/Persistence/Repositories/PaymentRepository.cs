using Microsoft.EntityFrameworkCore;
using Payment.API.Application.Interfaces;
using Payment.API.Infrastructure.Persistence;
using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace Payment.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPaymentRepository"/> for managing payment records.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentRepository"/>.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    /// <inheritdoc/>
    public async Task AddAsync(PaymentEntity payment) =>
        await _context.Payments.AddAsync(payment);

    /// <inheritdoc/>
    public void Update(PaymentEntity payment) =>
        _context.Payments.Update(payment);
}