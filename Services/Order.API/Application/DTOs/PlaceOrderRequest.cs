using Order.API.Domain.Enums;

namespace Order.API.Application.DTOs;

/// <summary>
/// Request DTO for placing a new food order from the API.
/// </summary>
public class PlaceOrderRequest
{
    /// <summary>Gets or sets the unique identifier of the restaurant to order from.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the display name of the restaurant.</summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the restaurant's logo image.</summary>
    public string RestaurantLogoUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the full delivery address for the order.</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment method selected by the customer.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets the list of menu items to include in the order.</summary>
    public List<OrderItemDto> Items { get; set; } = new();
}