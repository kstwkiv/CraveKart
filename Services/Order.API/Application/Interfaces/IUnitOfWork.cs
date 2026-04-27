namespace Order.API.Application.Interfaces;

public interface IUnitOfWork
{
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
}