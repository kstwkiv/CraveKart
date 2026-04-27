using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using Delivery.API.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Delivery.API.Application.Handlers;

public class UpdateLocationHandler : IRequestHandler<UpdateLocationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<DeliveryHub> _hubContext;

    public UpdateLocationHandler(IUnitOfWork unitOfWork, IHubContext<DeliveryHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

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
