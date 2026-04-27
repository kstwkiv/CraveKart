using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;
using Delivery.API.Application.Interfaces;
using Delivery.API.Hubs;
using FoodFleet.Shared.Events.Delivery;
using FoodFleet.Shared.Messaging.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Delivery.API.Application.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IHubContext<DeliveryHub> _hubContext;

    public DeliveryService(
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IHubContext<DeliveryHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _hubContext = hubContext;
    }

    public async Task<DeliveryDto> AssignAsync(AssignDeliveryCommand request, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Deliveries.GetAvailableAgentAsync()
            ?? throw new Exception("No available delivery agents.");

        agent.IsAvailable = false;
        _unitOfWork.Deliveries.UpdateAgent(agent);

        var delivery = new Domain.Entities.Delivery
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Status = "Assigned",
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.Deliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new DeliveryAssignedEvent
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            AgentName = agent.FullName,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);

        return ToDto(delivery);
    }

    public async Task<DeliveryDto> AssignToAgentAsync(AssignToAgentCommand request, CancellationToken cancellationToken = default)
    {
        var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId)
            ?? throw new Exception("Agent not found.");

        agent.IsAvailable = false;
        _unitOfWork.Deliveries.UpdateAgent(agent);

        var delivery = new Domain.Entities.Delivery
        {
            OrderId = request.OrderId,
            AgentId = agent.Id,
            CustomerId = Guid.Empty,   // not needed for self-pickup flow
            CustomerEmail = string.Empty,
            Status = "Assigned",
            AssignedAt = DateTime.UtcNow
        };

        await _unitOfWork.Deliveries.AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(delivery);
    }

    public async Task<bool> UpdateLocationAsync(UpdateLocationCommand request, CancellationToken cancellationToken = default)
    {
        var delivery = await _unitOfWork.Deliveries.GetByAgentIdAsync(request.AgentId);
        if (delivery == null) return false;

        delivery.CurrentLat = request.Lat;
        delivery.CurrentLng = request.Lng;
        _unitOfWork.Deliveries.Update(delivery);
        await _unitOfWork.SaveChangesAsync();

        await _hubContext.Clients
            .Group(delivery.OrderId.ToString())
            .SendAsync("LocationUpdated", new { orderId = delivery.OrderId, lat = request.Lat, lng = request.Lng }, cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(CompleteDeliveryCommand request, CancellationToken cancellationToken = default)
    {
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(request.OrderId);
        if (delivery == null) return false;

        delivery.Status = "Delivered";
        delivery.CompletedAt = DateTime.UtcNow;
        _unitOfWork.Deliveries.Update(delivery);

        var agent = delivery.Agent;
        if (agent != null)
        {
            agent.IsAvailable = true;
            agent.TotalDeliveries++;
            agent.TotalEarnings += 100; // ₹100 per delivery
            _unitOfWork.Deliveries.UpdateAgent(agent);
        }

        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new DeliveryCompletedEvent
        {
            OrderId = delivery.OrderId,
            AgentId = delivery.AgentId,
            CustomerId = delivery.CustomerId,
            CustomerEmail = delivery.CustomerEmail,
            CompletedAt = delivery.CompletedAt!.Value
        }, cancellationToken);

        return true;
    }

    public async Task<DeliveryDto?> GetByOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var delivery = await _unitOfWork.Deliveries.GetByOrderIdAsync(orderId);
        return delivery == null ? null : ToDto(delivery);
    }

    private static DeliveryDto ToDto(Domain.Entities.Delivery delivery) => new()
    {
        Id = delivery.Id,
        OrderId = delivery.OrderId,
        AgentId = delivery.AgentId,
        Status = delivery.Status,
        CurrentLat = delivery.CurrentLat,
        CurrentLng = delivery.CurrentLng,
        AssignedAt = delivery.AssignedAt,
        CompletedAt = delivery.CompletedAt
    };
}
