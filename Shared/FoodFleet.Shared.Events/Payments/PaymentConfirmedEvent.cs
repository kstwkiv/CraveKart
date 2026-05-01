using FoodFleet.Shared.Events;

namespace FoodFleet.Shared.Events.Payments;

public class PaymentConfirmedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime ConfirmedAt { get; set; } = IstClock.Now;
}