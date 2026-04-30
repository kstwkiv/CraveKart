using Delivery.API.Application.DTOs;

namespace Delivery.API.Application.Commands;

/// <summary>
/// Command to assign a delivery agent to an order automatically.
/// Triggers the system to find an available agent and create a delivery record.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to be delivered.</param>
/// <param name="CustomerId">The unique identifier of the customer who placed the order.</param>
/// <param name="CustomerEmail">The email address of the customer for delivery notifications.</param>
public record AssignDeliveryCommand(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail);
