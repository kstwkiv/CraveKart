using FoodFleet.Shared.Events.Orders;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;
using Order.API.Application.Commands;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Domain.Entities;
using Order.API.Domain.Enums;

namespace Order.API.Application.Handlers;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public PlaceOrderHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderDto> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var subTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        var deliveryFee = 30m;
        var tax = subTotal * 0.05m;

        var order = new Domain.Entities.Order
        {
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            RestaurantId = request.RestaurantId,
            DeliveryAddress = request.DeliveryAddress,
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatus.Placed,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Tax = tax,
            TotalAmount = subTotal + deliveryFee + tax,
            OrderItems = request.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Customizations = i.Customizations
            }).ToList()
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new OrderPlacedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CustomerEmail = order.CustomerEmail,
            PlacedAt = DateTime.UtcNow
        }, cancellationToken);

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            DeliveryAddress = order.DeliveryAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt,
            Items = request.Items
        };
    }
}