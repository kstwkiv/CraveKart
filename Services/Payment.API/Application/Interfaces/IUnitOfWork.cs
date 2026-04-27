namespace Payment.API.Application.Interfaces;

public interface IUnitOfWork
{
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync();
}