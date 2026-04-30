using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Orders;

public class OrderPlacedEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; } = IstClock.Now;
    public List<OrderPlacedItemEvent> Items { get; set; } = new();
}

public class OrderPlacedItemEvent
{
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Customizations { get; set; }
}