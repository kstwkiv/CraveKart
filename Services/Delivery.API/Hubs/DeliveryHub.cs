using Microsoft.AspNetCore.SignalR;

namespace Delivery.API.Hubs;

/// <summary>
/// SignalR hub for broadcasting real-time delivery location updates to connected clients.
/// Clients join an order-specific group to receive location events for that order.
/// </summary>
public class DeliveryHub : Hub
{
    /// <summary>
    /// Broadcasts a location update to all clients in the order's group.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order being tracked.</param>
    /// <param name="lat">The updated latitude of the delivery agent.</param>
    /// <param name="lng">The updated longitude of the delivery agent.</param>
    public async Task UpdateLocation(Guid orderId, double lat, double lng)
    {
        await Clients.Group(orderId.ToString())
            .SendAsync("LocationUpdated", lat, lng);
    }

    /// <summary>
    /// Adds the calling connection to the SignalR group for a specific order,
    /// enabling it to receive location updates for that order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to track.</param>
    public async Task JoinOrderGroup(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
    }
}