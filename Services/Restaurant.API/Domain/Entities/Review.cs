namespace Restaurant.API.Domain.Entities;

/// <summary>
/// Represents a customer review for a restaurant, linked to a specific order.
/// </summary>
public class Review
{
    /// <summary>Gets or sets the unique identifier of the review.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the unique identifier of the reviewed restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who wrote the review.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the display name of the customer at the time of review.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the star rating given by the customer (1–5).</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the optional text content of the review.</summary>
    public string? ReviewText { get; set; }

    /// <summary>Gets or sets the optional response from the restaurant owner.</summary>
    public string? OwnerResponse { get; set; }

    /// <summary>Gets or sets the IST timestamp when the review was created.</summary>
    public DateTime CreatedAt { get; set; } = IstClock.Now;

    /// <summary>Gets or sets the reviewed restaurant (navigation property).</summary>
    public Restaurant Restaurant { get; set; } = null!;
}
