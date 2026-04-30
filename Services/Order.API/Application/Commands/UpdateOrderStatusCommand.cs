using Order.API.Domain.Enums;

namespace Order.API.Application.Commands;

/// <summary>
/// Command to update the status of an existing order.
/// Triggers an <c>OrderStatusChangedEvent</c> upon successful update.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to update.</param>
/// <param name="NewStatus">The new status to set for the order.</param>
public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus);
