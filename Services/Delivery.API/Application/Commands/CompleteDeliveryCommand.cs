namespace Delivery.API.Application.Commands;

/// <summary>
/// Command to mark a delivery as completed and update agent statistics.
/// </summary>
/// <param name="OrderId">The unique identifier of the order whose delivery is being completed.</param>
public record CompleteDeliveryCommand(Guid OrderId);
