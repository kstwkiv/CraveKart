namespace Restaurant.API.Application.DTOs;

/// <summary>
/// Data transfer object representing a customer review returned to API consumers.
/// </summary>
public class ReviewDto
{
    /// <summary>Gets or sets the unique identifier of the review.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the unique identifier of the reviewed restaurant.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the unique identifier of the associated order.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the unique identifier of the customer who wrote the review.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the display name of the customer.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the star rating given (1–5).</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the optional text content of the review.</summary>
    public string? ReviewText { get; set; }

    /// <summary>Gets or sets the optional response from the restaurant owner.</summary>
    public string? OwnerResponse { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the review was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for submitting a new customer review.
/// </summary>
public class CreateReviewRequest
{
    /// <summary>Gets or sets the unique identifier of the restaurant being reviewed.</summary>
    public Guid RestaurantId { get; set; }

    /// <summary>Gets or sets the unique identifier of the order associated with this review.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the star rating (must be between 1 and 5).</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the optional text content of the review.</summary>
    public string? ReviewText { get; set; }
}

/// <summary>
/// Request DTO for a restaurant owner to respond to a customer review.
/// </summary>
public class RespondToReviewRequest
{
    /// <summary>Gets or sets the owner's response text.</summary>
    public string Response { get; set; } = string.Empty;
}
