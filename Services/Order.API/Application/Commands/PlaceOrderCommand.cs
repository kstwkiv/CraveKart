using Order.API.Application.DTOs;
using Order.API.Domain.Enums;

namespace Order.API.Application.Commands;

public record PlaceOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    Guid RestaurantId,
    string RestaurantName,
    string RestaurantLogoUrl,
    string DeliveryAddress,
    PaymentMethod PaymentMethod,
    List<OrderItemDto> Items);
