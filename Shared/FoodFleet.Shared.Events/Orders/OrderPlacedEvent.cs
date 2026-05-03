// 'using' — imports the FoodFleet.Shared.Events namespace (provides IstClock)
using FoodFleet.Shared.Events;

// 'namespace' — groups order-related events together under the Orders sub-domain
namespace FoodFleet.Shared.Events.Orders;

/// <summary>
/// Domain event published when a new order is placed by a customer.
/// Consumed by the Payment service (to initiate payment) and the Notification service (to send an order confirmation email).
/// </summary>
// 'public' — visible to all microservices that consume this event from the message bus
// 'class' — reference type; the entire object graph is serialised and sent as a message
public class OrderPlacedEvent
{
    /// <summary>Gets or sets the unique identifier of the placed order.</summary>
    // 'Guid' — globally unique identifier for the order; used to correlate events across services
    // '{ get; set; }' — auto-implemented property with compiler-generated backing field
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who placed the order.</summary>
    // 'Guid' — identifies the customer who placed the order
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the unique identifier of the restaurant being ordered from.</summary>
    // 'Guid' — identifies the restaurant that received the order
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the full delivery address for the order.</summary>
    // 'string' — text type; stores the delivery address as a human-readable string
    // '= string.Empty' — non-null default; avoids null reference exceptions on deserialisation
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Gets or sets the subtotal before fees and tax.</summary>
    // 'decimal' — base-10 floating-point type; preferred for monetary values to avoid rounding errors
    public decimal SubTotal { get; set; }

    /// <summary>Gets or sets the delivery fee charged.</summary>
    // 'decimal' — monetary value; base-10 arithmetic ensures exact cent-level precision
    public decimal DeliveryFee { get; set; }

    /// <summary>Gets or sets the tax amount (5% of subtotal).</summary>
    // 'decimal' — tax amount; decimal avoids the binary floating-point imprecision of double/float
    public decimal Tax { get; set; }

    /// <summary>Gets or sets the total amount charged (subtotal + delivery fee + tax).</summary>
    // 'decimal' — total charged to the customer (SubTotal + DeliveryFee + Tax)
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the payment method selected (e.g., "UpiNow", "CashOnDelivery").</summary>
    // 'string' — payment method name (e.g., "Card", "UPI", "COD")
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's email address for order notifications.</summary>
    // 'string' — customer email used by the Notification service to send order confirmation
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the IST timestamp when the order was placed.</summary>
    // 'DateTime' — value type capturing the exact moment the order was placed
    // 'IstClock.Now' — IST-aware timestamp for consistent time across all services
    public DateTime PlacedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the list of items included in the order.</summary>
    // 'List<T>' — generic ordered collection; holds all line items that belong to this order
    // 'new()' — target-typed new expression; initialises an empty list so the property is never null
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}

/// <summary>
/// Represents a single line item within an <see cref="OrderPlacedEvent"/>.
/// </summary>
// 'public' — this nested event DTO is accessible to all consumers
// 'class' — reference type representing a single line item within the order event
public class OrderPlacedItemEvent
{
    /// <summary>Gets or sets the display name of the menu item.</summary>
    // 'string' — name of the menu item ordered
    public string MenuItemName { get; set; } = string.Empty;

    /// <summary>Gets or sets the quantity of this item ordered.</summary>
    // 'int' — 32-bit signed integer; sufficient for item quantities in a single order
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price of the item at the time of ordering.</summary>
    // 'decimal' — price per unit; decimal type ensures exact monetary arithmetic
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets any special customization notes for this item.</summary>
    // 'string?' — nullable string: customisations are optional and may not be present
    // 'null' — the absence of a value; signals no customisations were requested
    public string? Customizations { get; set; }
}
