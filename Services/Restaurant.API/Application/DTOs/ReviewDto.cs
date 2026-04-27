namespace Restaurant.API.Application.DTOs;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public string? OwnerResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public Guid RestaurantId { get; set; }
    public Guid OrderId { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
}

public class RespondToReviewRequest
{
    public string Response { get; set; } = string.Empty;
}
