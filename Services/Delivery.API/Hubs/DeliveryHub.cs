using Microsoft.AspNetCore.SignalR;

namespace Delivery.API.Hubs;

public class DeliveryHub : Hub
{
    public async Task UpdateLocation(Guid orderId, double lat, double lng)
    {
        await Clients.Group(orderId.ToString())
            .SendAsync("LocationUpdated", lat, lng);
    }

    public async Task JoinOrderGroup(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId.ToString());
    }
}