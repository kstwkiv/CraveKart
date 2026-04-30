using Order.API.Domain.Enums;

namespace Order.API.Application.DTOs;

/// <summary>
/// Data transfer object representing an order returned to API consumers.
/// </summary>
public class OrderDto
{
    /// <summary>Gets or sets the unique identifier of the order.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the display name of the restaurant.</summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the restaurant's logo image.</summary>
    public string RestaurantLogoUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the delivery address for the order.</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status of the order.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Gets or sets the total amount charged for the order including fees and tax.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the payment method used (e.g., "Card", "CashOnDelivery").</summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp when the order was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the list of items included in the order.</summary>
    public List<OrderItemDto> Items { get; set; } = new();
}