namespace Order.API.Domain.Entities;

/// <summary>
/// Represents a single line item within a food order.
/// </summary>
public class OrderItem
{
    /// <summary>Gets or sets the unique identifier of the order item.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the parent order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the menu item.</summary>
    public Guid MenuItemId { get; set; }

    /// <summary>Gets or sets the display name of the menu item at the time of ordering.</summary>
    public string MenuItemName { get; set; } = string.Empty;

    /// <summary>Gets or sets the quantity of this item ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price of the item at the time of ordering.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets any special customization notes for this item.</summary>
    public string? Customizations { get; set; }

    /// <summary>Gets or sets the parent order (navigation property).</summary>
    public Order Order { get; set; } = null!;
}