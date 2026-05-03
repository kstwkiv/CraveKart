using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Application.Commands;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Controllers;

/// <summary>
/// API controller for customer and restaurant-facing order operations.
/// Provides endpoints for placing, retrieving, cancelling, and updating orders.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>
    /// Initializes a new instance of <see cref="OrdersController"/>.
    /// </summary>
    /// <param name="orderService">The order service for business logic.</param>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Places a new food order for the authenticated customer.
    /// </summary>
    /// <param name="request">The order request containing restaurant, items, and delivery details.</param>
    /// <returns>The created order DTO.</returns>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var customerEmail = User.FindFirstValue(ClaimTypes.Email)!;

        var command = new PlaceOrderCommand(
            customerId,
            customerEmail,
            request.RestaurantId,
            request.RestaurantName,
            request.RestaurantLogoUrl,
            request.DeliveryAddress,
            request.PaymentMethod,
            request.Items);

        var result = await _orderService.PlaceOrderAsync(command);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order DTO, or 404 if not found.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the order history for a specific customer.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A list of order DTOs for the customer.</returns>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetHistory(Guid customerId)
    {
        var result = await _orderService.GetHistoryAsync(customerId);
        return Ok(result);
    }

    /// <summary>
    /// Cancels an order. Accessible by Customer and Admin roles.
    /// </summary>
    /// <param name="id">The unique identifier of the order to cancel.</param>
    /// <returns>200 OK on success, or 400 if the order cannot be cancelled.</returns>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _orderService.CancelAsync(new CancelOrderCommand(id, userId));
        if (!result) return BadRequest("Cannot cancel order.");
        return Ok("Order cancelled.");
    }

    /// <summary>
    /// Retrieves all orders for a specific restaurant. Accessible by RestaurantOwner and Admin roles.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>A list of order DTOs for the restaurant.</returns>
    [HttpGet("restaurant/{restaurantId}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
    {
        var orders = await _orderService.GetByRestaurantAsync(restaurantId);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves all orders with Ready status for delivery agents to pick up.
    /// </summary>
    /// <returns>A list of ready order DTOs.</returns>
    [HttpGet("ready")]
    [Authorize(Roles = "DeliveryAgent,Admin")]
    public async Task<IActionResult> GetReadyOrders()
    {
        var orders = await _orderService.GetByStatusAsync(OrderStatus.Ready);
        return Ok(orders);
    }

    /// <summary>
    /// Updates the status of an order. Accessible by RestaurantOwner, Admin, and DeliveryAgent roles.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="status">The new status to set.</param>
    /// <returns>200 OK on success, or 404 if not found.</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "RestaurantOwner,Admin,DeliveryAgent")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
    {
        var result = await _orderService.UpdateStatusAsync(new UpdateOrderStatusCommand(id, status));
        if (!result) return NotFound();
        return Ok("Status updated.");
    }
}
