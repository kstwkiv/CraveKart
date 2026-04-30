using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using Delivery.API.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Delivery.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="UpdateLocationCommand"/> requests.
/// Updates the delivery's GPS coordinates and broadcasts the new location via SignalR.
/// </summary>
public class UpdateLocationHandler : IRequestHandler<UpdateLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<DeliveryHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateLocationHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="hubContext">The SignalR hub context for broadcasting location updates.</param>
    public UpdateLocationHandler(IUnitOfWork unitOfWork, IHubContext<DeliveryHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Handles the location update, persists the new coordinates, and pushes a real-time
    /// "LocationUpdated" event to all SignalR clients tracking the associated order.
    /// </summary>
    /// <param name="request">The command containing the agent ID and new coordinates.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the location was updated; <c>false</c> if no delivery was found.</returns>
    public async Task<bool> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(request.DeliveryId);
        if (delivery == null) return false;

        delivery.CurrentLat = request.Lat;
        delivery.CurrentLng = request.Lng;
        _unitOfWork.Deliveries.Update(delivery);
        await _unitOfWork.SaveChangesAsync();

        // Push real-time location to all clients tracking this order
        await _hubContext.Clients
            .Group(request.DeliveryId.ToString())
            .SendAsync("LocationUpdated", request.Lat, request.Lng, cancellationToken);

        return true;
    }
}
