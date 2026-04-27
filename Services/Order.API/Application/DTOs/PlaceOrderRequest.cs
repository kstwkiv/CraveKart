using Order.API.Domain.Enums;

namespace Order.API.Application.DTOs;

public class PlaceOrderRequest
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantLogoUrl { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}