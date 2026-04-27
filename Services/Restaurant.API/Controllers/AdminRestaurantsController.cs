using FoodFleet.Shared.Events.Restaurants;
using FoodFleet.Shared.Messaging.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/v1/admin/restaurants")]
[Authorize(Roles = "Admin")]
public class AdminRestaurantsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public AdminRestaurantsController(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

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

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var restaurants = await _unitOfWork.Restaurants.GetByStatusAsync(RestaurantStatus.Pending);
        return Ok(restaurants.Select(ToDto));
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Active;
        restaurant.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new RestaurantApprovedEvent
        {
            RestaurantId = restaurant.Id,
            OwnerId = restaurant.OwnerId,
            RestaurantName = restaurant.Name,
            ApprovedAt = DateTime.UtcNow
        });

        return Ok(ToDto(restaurant));
    }

    [HttpPatch("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Rejected;
        restaurant.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = $"Restaurant rejected. Reason: {request.Reason}" });
    }

    [HttpPatch("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] RejectRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.Status = RestaurantStatus.Suspended;
        restaurant.IsOpen = false;
        restaurant.UpdatedAt = DateTime.UtcNow;
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

public class RejectRestaurantRequest
{
    public string Reason { get; set; } = string.Empty;
}
