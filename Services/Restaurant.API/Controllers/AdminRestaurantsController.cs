using FoodFleet.Shared.Events.Restaurants;
using FoodFleet.Shared.Messaging.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Controllers;

/// <summary>
/// API controller for admin-level restaurant management operations.
/// Provides endpoints for listing, approving, rejecting, and suspending restaurants.
/// Accessible by Admin role only.
/// </summary>
[ApiController]
[Route("api/v1/admin/restaurants")]
[Authorize(Roles = "Admin")]
public class AdminRestaurantsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminRestaurantsController"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public AdminRestaurantsController(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Retrieves all restaurants, optionally filtered by status.
    /// </summary>
    /// <param name="status">Optional status filter (e.g., "Pending", "Active").</param>
    /// <returns>A list of restaurant DTOs.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (Enum.TryParse<RestaurantStatus>(status, ignoreCase: true, out var parsed))
        {
            var filtered = await _unitOfWork.Restaurants.GetByStatusAsync(parsed);
            return Ok(filtered.Select(ToDto));
        }

        var all = await _unitOfWork.Restaurants.GetAllAsync();
        return Ok(all.Select(ToDto));
    }

    /// <summary>
    /// Retrieves all restaurants with Pending status awaiting admin review.
    /// </summary>
    /// <returns>A list of pending restaurant DTOs.</returns>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var restaurants = await _unitOfWork.Restaurants.GetByStatusAsync(RestaurantStatus.Pending);
        return Ok(restaurants.Select(ToDto));
    }

    /// <summary>
    /// Approves a restaurant, setting its status to Active and publishing a <see cref="RestaurantApprovedEvent"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant to approve.</param>
    /// <returns>The updated restaurant DTO, or 404 if not found.</returns>
    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Active;
        restaurant.UpdatedAt = IstClock.Now;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new RestaurantApprovedEvent
        {
            RestaurantId = restaurant.Id,
            OwnerId = restaurant.OwnerId,
            RestaurantName = restaurant.Name,
            ApprovedAt = IstClock.Now
        });

        return Ok(ToDto(restaurant));
    }

    /// <summary>
    /// Rejects a restaurant application with a reason.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant to reject.</param>
    /// <param name="request">The request containing the rejection reason.</param>
    /// <returns>200 OK with a confirmation message, or 404 if not found.</returns>
    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Rejected;
        restaurant.UpdatedAt = IstClock.Now;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = $"Restaurant rejected. Reason: {request.Reason}" });
    }

    /// <summary>
    /// Suspends an active restaurant with a reason, closing it immediately.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant to suspend.</param>
    /// <param name="request">The request containing the suspension reason.</param>
    /// <returns>200 OK with a confirmation message, or 404 if not found.</returns>
    [HttpPatch("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] RejectRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Suspended;
        restaurant.IsOpen = false;
        restaurant.UpdatedAt = IstClock.Now;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = $"Restaurant suspended. Reason: {request.Reason}" });
    }

    private static RestaurantDto ToDto(Domain.Entities.Restaurant r) => new()
    {
        Id = r.Id,
        OwnerId = r.OwnerId,
        Name = r.Name,
        Description = r.Description,
        Address = r.Address,
        CuisineTypes = r.CuisineTypes,
        Status = r.Status.ToString(),
        IsOpen = r.IsOpen,
        MinimumOrderAmount = r.MinimumOrderAmount,
        EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
        AverageRating = r.AverageRating,
        TotalReviews = r.TotalReviews
    };
}

/// <summary>
/// Request DTO for rejecting or suspending a restaurant with a reason.
/// </summary>
public class RejectRestaurantRequest
{
    /// <summary>Gets or sets the reason for the rejection or suspension.</summary>
    public string Reason { get; set; } = string.Empty;
}
