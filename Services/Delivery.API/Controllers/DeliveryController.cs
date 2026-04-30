using System.Security.Claims;
using Delivery.API.Application.Commands;
using Delivery.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Delivery.API.Controllers;

/// <summary>
/// API controller for managing delivery lifecycle operations.
/// Provides endpoints for assigning, tracking, and completing deliveries.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="DeliveryController"/>.
    /// </summary>
    /// <param name="deliveryService">The delivery service for business logic.</param>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public DeliveryController(IDeliveryService deliveryService, IUnitOfWork unitOfWork)
    {
        _deliveryService = deliveryService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Assigns an available delivery agent to an order. Admin only.
    /// </summary>
    /// <param name="command">The command containing order and customer details.</param>
    /// <returns>The created delivery assignment DTO.</returns>
    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign([FromBody] AssignDeliveryCommand command)
    {
        var result = await _deliveryService.AssignAsync(command);
        return Ok(result);
    }

    /// <summary>
    /// Agent self-assigns a Ready order — no admin needed.
    /// </summary>
    [HttpPost("pickup/{orderId}")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> Pickup(Guid orderId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound("Agent profile not found. Please register first.");
        if (!agent.IsAvailable) return BadRequest("You already have an active delivery.");

        // Check there isn't already a delivery for this order
        var existing = await _unitOfWork.Deliveries.GetByOrderIdAsync(orderId);
        if (existing != null) return Conflict("This order has already been picked up.");

        var result = await _deliveryService.AssignToAgentAsync(
            new AssignToAgentCommand(orderId, agent.Id),
            CancellationToken.None);
        return Ok(result);
    }

    [HttpPatch("location")]
    [Authorize(Roles = "DeliveryAgent")]
    /// <summary>
    /// Updates the real-time GPS location of the authenticated agent's active delivery.
    /// </summary>
    /// <param name="request">The request containing the new latitude and longitude.</param>
    /// <returns>200 OK on success, or 404 if no active delivery is found.</returns>
    public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound("Agent profile not found.");

        var result = await _deliveryService.UpdateLocationAsync(
            new UpdateLocationCommand(agent.Id, request.Lat, request.Lng));
        if (!result) return NotFound("No active delivery found for this agent.");
        return Ok("Location updated.");
    }

    [HttpPatch("{orderId}/complete")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    /// <summary>
    /// Marks a delivery as completed for the given order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to complete.</param>
    /// <returns>200 OK on success, or 404 if the delivery is not found.</returns>
    public async Task<IActionResult> Complete(Guid orderId)
    {
        var result = await _deliveryService.CompleteAsync(new CompleteDeliveryCommand(orderId));
        if (!result) return NotFound();
        return Ok("Delivery completed.");
    }

    [HttpGet("{orderId}")]
    /// <summary>
    /// Retrieves the delivery record for a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The delivery DTO, or 404 if not found.</returns>
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var delivery = await _deliveryService.GetByOrderAsync(orderId);
        if (delivery == null) return NotFound();
        return Ok(delivery);
    }

    [HttpGet("my")]
    [Authorize(Roles = "DeliveryAgent")]
    /// <summary>
    /// Retrieves the active delivery assigned to the authenticated agent.
    /// </summary>
    /// <returns>The active delivery details, or 404 if none found.</returns>
    public async Task<IActionResult> GetMyDelivery()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        if (agent == null) return NotFound("Agent profile not found.");

        var delivery = await _unitOfWork.Deliveries.GetByAgentIdAsync(agent.Id);
        if (delivery == null) return NotFound("No active delivery.");

        return Ok(new
        {
            delivery.Id,
            delivery.OrderId,
            delivery.Status,
            delivery.CurrentLat,
            delivery.CurrentLng,
            delivery.AssignedAt,
            delivery.CompletedAt
        });
    }
}

/// <summary>Request model for updating a delivery agent's real-time GPS location.</summary>
public class LocationUpdateRequest
{
    /// <summary>Gets or sets the latitude coordinate.</summary>
    public double Lat { get; set; }

    /// <summary>Gets or sets the longitude coordinate.</summary>
    public double Lng { get; set; }
}
