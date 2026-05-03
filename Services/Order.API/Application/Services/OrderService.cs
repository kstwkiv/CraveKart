using FoodFleet.Shared.Events.Orders;
using FoodFleet.Shared.Messaging.Interfaces;
using Order.API.Application.Commands;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Domain.Entities;
using Order.API.Domain.Enums;

namespace Order.API.Application.Services;

/// <summary>
/// Application service implementing <see cref="IOrderService"/> for core order management operations.
/// Handles order placement, retrieval, cancellation, and status updates, publishing domain events
/// via <see cref="IEventPublisher"/> for downstream service coordination.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="OrderService"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public OrderService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand request, CancellationToken cancellationToken = default)
    {
        var subTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        var deliveryFee = 30m;
        var tax = subTotal * 0.05m;

        var order = new Domain.Entities.Order
        {
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            RestaurantId = request.RestaurantId,
            RestaurantName = request.RestaurantName,
            RestaurantLogoUrl = request.RestaurantLogoUrl,
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
            DeliveryAddress = order.DeliveryAddress,
            SubTotal = order.SubTotal,
            DeliveryFee = order.DeliveryFee,
            Tax = order.Tax,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CustomerEmail = order.CustomerEmail,
            PlacedAt = IstClock.Now,
            Items = order.OrderItems.Select(i => new OrderPlacedItemEvent
            {
                MenuItemName = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Customizations = i.Customizations
            }).ToList()
        }, cancellationToken);

        return ToDto(order, request.Items);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        return order == null ? null : ToDto(order);
    }

    public async Task<List<OrderDto>> GetHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId);
        return orders.Select(o => ToDto(o)).ToList();
    }

    public async Task<bool> CancelAsync(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        if (order.Status != OrderStatus.Placed && order.Status != OrderStatus.Confirmed)
            throw new Exception("Order cannot be cancelled at this stage.");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = IstClock.Now;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new OrderCancelledEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            Reason = "Cancelled by customer",
            CancelledAt = IstClock.Now
        }, cancellationToken);

        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByRestaurantIdAsync(restaurantId);
        return orders.Select(o => ToDto(o)).ToList();
    }

    public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByStatusAsync(status);
        return orders.Select(o => ToDto(o)).ToList();
    }

    public async Task<bool> UpdateStatusAsync(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        var oldStatus = order.Status.ToString();
        order.Status = request.NewStatus;
        order.UpdatedAt = IstClock.Now;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = order.CustomerEmail,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus.ToString(),
            ChangedAt = IstClock.Now
        }, cancellationToken);

        return true;
    }

    private static OrderDto ToDto(Domain.Entities.Order order, List<OrderItemDto>? items = null) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        RestaurantId = order.RestaurantId,
        RestaurantName = order.RestaurantName,
        RestaurantLogoUrl = order.RestaurantLogoUrl,
        DeliveryAddress = order.DeliveryAddress,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        PaymentMethod = order.PaymentMethod.ToString(),
        CreatedAt = order.CreatedAt,
        Items = items ?? order.OrderItems.Select(i => new OrderItemDto
        {
            MenuItemId = i.MenuItemId,
            MenuItemName = i.MenuItemName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Customizations = i.Customizations
        }).ToList()
    };
}
