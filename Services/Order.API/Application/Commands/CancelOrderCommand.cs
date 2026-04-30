namespace Order.API.Application.Commands;

/// <summary>
/// Command to cancel an existing order. Only orders in Placed or Confirmed status can be cancelled.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to cancel.</param>
/// <param name="CustomerId">The unique identifier of the customer requesting the cancellation.</param>
public record CancelOrderCommand(Guid OrderId, Guid CustomerId);
