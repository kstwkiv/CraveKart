using System.Security.Claims;
using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.API.Controllers;

[ApiController]
[Route("api/v1/agents")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AgentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // POST /api/v1/agents/register — DeliveryAgent registers their profile
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

public class RegisterAgentRequest
{
    public string VehicleType { get; set; } = string.Empty;
}

public class UpdateVehicleRequest
{
    public string VehicleType { get; set; } = string.Empty;
}

public class UpdateLocationRequest
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
