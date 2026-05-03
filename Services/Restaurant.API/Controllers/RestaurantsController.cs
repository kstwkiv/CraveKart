using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;

namespace Restaurant.API.Controllers;

/// <summary>
/// API controller for public and owner-facing restaurant operations.
/// Provides endpoints for browsing, creating, and managing restaurant listings.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="RestaurantsController"/>.
    /// </summary>
    /// <param name="restaurantService">The restaurant service for business logic.</param>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public RestaurantsController(IRestaurantService restaurantService, IUnitOfWork unitOfWork)
    {
        _restaurantService = restaurantService;
        _unitOfWork = unitOfWork;
    }

    // GET /api/v1/restaurants  — public
    /// <summary>
    /// Retrieves all active restaurants, optionally filtered by a search term. Publicly accessible.
    /// </summary>
    /// <param name="search">Optional search term to filter by name or cuisine type.</param>
    /// <returns>A list of active restaurant DTOs.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var result = await _restaurantService.GetAllAsync(search);
        return Ok(result);
    }

    // GET /api/v1/restaurants/my  — owner sees all their restaurants regardless of status
    /// <summary>
    /// Retrieves all restaurants owned by the authenticated restaurant owner, regardless of status.
    /// </summary>
    /// <returns>A list of restaurant DTOs for the authenticated owner.</returns>
    [HttpGet("my")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<IActionResult> GetMine()
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var restaurants = await _unitOfWork.Restaurants.GetAllByOwnerIdAsync(ownerId);
        return Ok(restaurants.Select(r => new RestaurantDto
        {
            Id = r.Id,
            OwnerId = r.OwnerId,
            Name = r.Name,
            Description = r.Description,
            Address = r.Address,
            CuisineTypes = r.CuisineTypes,
            OperatingHours = r.OperatingHours,
            AverageRating = r.AverageRating,
            TotalReviews = r.TotalReviews,
            IsOpen = r.IsOpen,
            EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
            MinimumOrderAmount = r.MinimumOrderAmount,
            Status = r.Status.ToString(),
            LogoUrl = r.LogoUrl
        }));
    }

    // GET /api/v1/restaurants/{id}  — public
    /// <summary>
    /// Retrieves a single restaurant by its unique identifier. Publicly accessible.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant.</param>
    /// <returns>The restaurant DTO, or 404 if not found.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _restaurantService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST /api/v1/restaurants  — RestaurantOwner or Admin
    /// <summary>
    /// Creates a new restaurant listing with Pending status. Accessible by RestaurantOwner and Admin roles.
    /// </summary>
    /// <param name="request">The request containing restaurant details.</param>
    /// <returns>The created restaurant DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest request)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ownerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        var command = new CreateRestaurantCommand(
            ownerId,
            ownerEmail,
            request.Name,
            request.Description,
            request.Address,
            request.Lat,
            request.Lng,
            request.CuisineTypes,
            request.OperatingHours,
            request.MinimumOrderAmount,
            request.EstimatedDeliveryMinutes,
            request.LogoUrl);

        var result = await _restaurantService.CreateAsync(command);
        return Ok(result);
    }

    // PATCH /api/v1/restaurants/{id}/availability
    /// <summary>
    /// Toggles the open/closed availability status of a restaurant. Accessible by RestaurantOwner and Admin roles.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant.</param>
    /// <returns>The new availability state, or 404 if not found.</returns>
    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        var isOpen = await _restaurantService.ToggleAvailabilityAsync(id);
        if (isOpen == null) return NotFound();
        return Ok(new { Id = id, IsOpen = isOpen.Value });
    }

    // PUT /api/v1/restaurants/{id}  — owner updates their own restaurant
    /// <summary>
    /// Updates an existing restaurant's details. Only the owning RestaurantOwner can update their restaurant.
    /// </summary>
    /// <param name="id">The unique identifier of the restaurant to update.</param>
    /// <param name="request">The request containing updated restaurant details.</param>
    /// <returns>The updated restaurant DTO, or 404 if not found or not owned by the caller.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "RestaurantOwner")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRestaurantRequest request)
    {
        var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UpdateRestaurantCommand(
            id,
            request.Name,
            request.Description,
            request.Address,
            request.CuisineTypes,
            request.OperatingHours,
            request.MinimumOrderAmount,
            request.EstimatedDeliveryMinutes,
            request.LogoUrl);

        var result = await _restaurantService.UpdateAsync(id, ownerId, command);
        if (result == null) return NotFound("Restaurant not found or you don't own it.");
        return Ok(result);
    }
}

/// <summary>
/// Request DTO for updating an existing restaurant's details.
/// </summary>
public class UpdateRestaurantRequest
{
    /// <summary>Gets or sets the new display name of the restaurant.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the new description of the restaurant.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the new physical address of the restaurant.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the updated comma-separated cuisine types.</summary>
    public string CuisineTypes { get; set; } = string.Empty;

    /// <summary>Gets or sets the updated operating hours description.</summary>
    public string OperatingHours { get; set; } = string.Empty;

    /// <summary>Gets or sets the updated minimum order amount.</summary>
    public double MinimumOrderAmount { get; set; }

    /// <summary>Gets or sets the updated estimated delivery time in minutes.</summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>Gets or sets the optional updated URL of the restaurant's logo image.</summary>
    public string? LogoUrl { get; set; }
}
