namespace Delivery.API.Application.Commands;

/// <summary>
/// Command to update the real-time GPS location of a delivery agent during an active delivery.
/// Triggers a SignalR broadcast to all clients tracking the associated order.
/// </summary>
/// <param name="AgentId">The unique identifier of the delivery agent.</param>
/// <param name="Lat">The current latitude coordinate of the agent.</param>
/// <param name="Lng">The current longitude coordinate of the agent.</param>
public record UpdateLocationCommand(
    Guid AgentId,
    double Lat,
    double Lng);
