using System.Security.Claims;
using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.API.Controllers;

/// <summary>
/// API controller for managing delivery agent profiles.
/// Provides endpoints for agent registration, profile management, and admin listing.
/// </summary>
[ApiController]
[Route("api/v1/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="AgentsController"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public AgentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Registers a new delivery agent profile for the authenticated user.
    /// </summary>
    /// <param name="request">The registration request containing vehicle type.</param>
    /// <returns>The created agent profile, or 409 Conflict if a profile already exists.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var fullName = User.FindFirstValue(ClaimTypes.Name)!;

        var existing = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (existing != null)
            return Conflict("Agent profile already exists.");

        var agent = new DeliveryAgent
        {
            UserId = userId,
            FullName = fullName,
            VehicleType = request.VehicleType,
            IsAvailable = true
        };

        await _unitOfWork.Agents.AddAsync(agent);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMyProfile), ToDto(agent));
    }

    // GET /api/v1/agents/me
    /// <summary>
    /// Retrieves the authenticated agent's own profile.
    /// </summary>
    /// <returns>The agent profile DTO, or 404 if not registered.</returns>
    [HttpGet("me")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound("Agent profile not found. Please register first.");
        return Ok(ToDto(agent));
    }

    // PATCH /api/v1/agents/me/availability
    /// <summary>
    /// Toggles the availability status of the authenticated agent.
    /// </summary>
    /// <returns>The updated availability state.</returns>
    [HttpPatch("me/availability")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> ToggleAvailability()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound();

        agent.IsAvailable = !agent.IsAvailable;
        _unitOfWork.Agents.Update(agent);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new { agent.Id, agent.IsAvailable });
    }

    // PATCH /api/v1/agents/me/vehicle — Update vehicle type
    /// <summary>
    /// Updates the vehicle type for the authenticated agent.
    /// </summary>
    /// <param name="request">The request containing the new vehicle type.</param>
    /// <returns>The updated agent profile, or 400/404 on failure.</returns>
    [HttpPatch("me/vehicle")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleType))
            return BadRequest("Vehicle type is required.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound();

        agent.VehicleType = request.VehicleType.Trim();
        _unitOfWork.Agents.Update(agent);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(agent));
    }

    // PATCH /api/v1/agents/me/location — Update agent's own standing location
    /// <summary>
    /// Updates the standing GPS location of the authenticated agent.
    /// </summary>
    /// <param name="request">The request containing the new latitude and longitude.</param>
    /// <returns>The updated agent profile.</returns>
    [HttpPatch("me/location")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound();

        agent.CurrentLat = request.Lat;
        agent.CurrentLng = request.Lng;
        _unitOfWork.Agents.Update(agent);
        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(agent));
    }

    // GET /api/v1/agents — Admin only
    /// <summary>
    /// Retrieves all registered delivery agents. Accessible by Admin only.
    /// </summary>
    /// <returns>A list of all agent profile DTOs.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _unitOfWork.Agents.GetAllAsync();
        return Ok(agents.Select(ToDto));
    }

    private static object ToDto(DeliveryAgent a) => new
    {
        a.Id,
        a.UserId,
        a.FullName,
        a.VehicleType,
        a.IsAvailable,
        a.TotalDeliveries,
        a.TotalEarnings,
        a.CurrentLat,
        a.CurrentLng,
        a.CreatedAt
    };
}

/// <summary>Request model for registering a new delivery agent profile.</summary>
public class RegisterAgentRequest
{
    /// <summary>Gets or sets the type of vehicle used by the agent (e.g., "Bike", "Car").</summary>
    public string VehicleType { get; set; } = string.Empty;
}

/// <summary>Request model for updating a delivery agent's vehicle type.</summary>
public class UpdateVehicleRequest
{
    /// <summary>Gets or sets the new vehicle type for the agent.</summary>
    public string VehicleType { get; set; } = string.Empty;
}

/// <summary>Request model for updating a delivery agent's GPS location.</summary>
public class UpdateLocationRequest
{
    /// <summary>Gets or sets the latitude coordinate.</summary>
    public double Lat { get; set; }

    /// <summary>Gets or sets the longitude coordinate.</summary>
    public double Lng { get; set; }
}
