using Order.API.Domain.Enums;

namespace Order.API.Domain.Entities;

/// <summary>
/// Represents a food order placed by a customer in the Order bounded context.
/// </summary>
public class Order
{
    /// <summary>Gets or sets the unique identifier of the order.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the display name of the restaurant at the time of ordering.</summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL of the restaurant's logo image.</summary>
    public string RestaurantLogoUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the assigned delivery agent. Null until assigned.</summary>
    public Guid? DeliveryAgentId { get; set; }

    /// <summary>Gets or sets the full delivery address for the order.</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status of the order.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    /// <summary>Gets or sets the subtotal before fees and tax.</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Gets or sets the delivery fee charged for the order.</summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>Gets or sets the tax amount (5% of subtotal).</summary>
    public decimal Tax { get; set; }

    /// <summary>Gets or sets the total amount charged (subtotal + delivery fee + tax).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the payment method selected by the customer.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets the Stripe payment intent ID for card payments. Null for cash orders.</summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>Gets or sets the customer's email address for order notifications.</summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the order was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the IST timestamp when the order was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the collection of items included in this order.</summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}