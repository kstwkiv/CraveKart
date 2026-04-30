namespace Order.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a single item within an order.
/// </summary>
public class OrderItemDto
{
    /// <summary>Gets or sets the unique identifier of the menu item.</summary>
    public Guid MenuItemId { get; set; }

    /// <summary>Gets or sets the display name of the menu item at the time of ordering.</summary>
    public string MenuItemName { get; set; } = string.Empty;

    /// <summary>Gets or sets the quantity of this item ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price of the item at the time of ordering.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets any special customization notes for this item (e.g., "No onions").</summary>
    public string? Customizations { get; set; }
}