namespace FoodFleet.Shared.Events.Payments;

public class PaymentConfirmedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;
}