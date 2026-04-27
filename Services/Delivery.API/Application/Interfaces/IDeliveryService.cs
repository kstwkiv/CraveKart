using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;

namespace Delivery.API.Application.Interfaces;

public interface IDeliveryService
{
    Task<DeliveryDto> AssignAsync(AssignDeliveryCommand request, CancellationToken cancellationToken = default);
    Task<DeliveryDto> AssignToAgentAsync(AssignToAgentCommand request, CancellationToken cancellationToken = default);
    Task<bool> UpdateLocationAsync(UpdateLocationCommand request, CancellationToken cancellationToken = default);
    Task<bool> CompleteAsync(CompleteDeliveryCommand request, CancellationToken cancellationToken = default);
    Task<DeliveryDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
