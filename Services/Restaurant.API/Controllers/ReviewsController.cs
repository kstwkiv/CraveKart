using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/v1/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET /api/v1/reviews/restaurant/{restaurantId}
    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
    {
        var reviews = await _unitOfWork.Reviews.GetByRestaurantIdAsync(restaurantId);
        return Ok(reviews.Select(ToDto));
    }

    // POST /api/v1/reviews
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest("Rating must be between 1 and 5.");

        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var customerName = User.FindFirstValue(ClaimTypes.Name) ?? "Customer";

        var exists = await _unitOfWork.Reviews.ExistsForOrderAsync(request.OrderId, customerId);
        if (exists)
            return Conflict("You have already reviewed this order.");

        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(request.RestaurantId);
        if (restaurant == null) return NotFound("Restaurant not found.");

        var review = new Review
        {
            RestaurantId = request.RestaurantId,
            OrderId = request.OrderId,
            CustomerId = customerId,
            CustomerName = customerName,
            Rating = request.Rating,
            ReviewText = request.ReviewText
        };

        await _unitOfWork.Reviews.AddAsync(review);

        // Recalculate average rating
        var allReviews = (await _unitOfWork.Reviews.GetByRestaurantIdAsync(request.RestaurantId)).ToList();
        allReviews.Add(review);
        restaurant.AverageRating = allReviews.Average(r => r.Rating);
        restaurant.TotalReviews = allReviews.Count;
        _unitOfWork.Restaurants.Update(restaurant);

        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByRestaurant), new { restaurantId = review.RestaurantId }, ToDto(review));
    }

    // POST /api/v1/reviews/{id}/response
    [HttpPost("{id}/response")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondToReviewRequest request)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null) return NotFound();

        review.OwnerResponse = request.Response;
        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(review));
    }

    // DELETE /api/v1/reviews/{id}  (Admin only)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null) return NotFound();

        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(review.RestaurantId);

        _unitOfWork.Reviews.Delete(review);
        await _unitOfWork.SaveChangesAsync();

        // Recalculate rating after deletion
        if (restaurant != null)
        {
            var remaining = (await _unitOfWork.Reviews.GetByRestaurantIdAsync(restaurant.Id)).ToList();
            restaurant.AverageRating = remaining.Any() ? remaining.Average(r => r.Rating) : 0;
            restaurant.TotalReviews = remaining.Count;
            _unitOfWork.Restaurants.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();
        }

        return NoContent();
    }

    private static ReviewDto ToDto(Review r) => new()
    {
        Id = r.Id,
        RestaurantId = r.RestaurantId,
        OrderId = r.OrderId,
        CustomerId = r.CustomerId,
        CustomerName = r.CustomerName,
        Rating = r.Rating,
        ReviewText = r.ReviewText,
        OwnerResponse = r.OwnerResponse,
        CreatedAt = r.CreatedAt
    };
}
