using Microsoft.EntityFrameworkCore;
using Payment.API.Application.Interfaces;
using Payment.API.Infrastructure.Persistence;
using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace Payment.API.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task AddAsync(PaymentEntity payment) =>
        await _context.Payments.AddAsync(payment);

    public void Update(PaymentEntity payment) =>
        _context.Payments.Update(payment);
}