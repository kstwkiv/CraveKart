namespace Payment.API.Application.Interfaces;

/// <summary>
/// Unit of Work interface for the Payment bounded context.
/// Coordinates transactional access to payment repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Gets the repository for payment records.</summary>
    IPaymentRepository Payments { get; }

    /// <summary>Persists all pending changes to the database.</summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}