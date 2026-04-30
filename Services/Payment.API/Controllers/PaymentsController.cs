using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;
using Payment.API.Application.Interfaces;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpPost("process")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paymentService.ProcessAsync(new ProcessPaymentCommand(
            request.OrderId,
            customerId,
            request.CustomerEmail ?? string.Empty,
            request.Amount,
            request.PaymentMethod));
        return Ok(result);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var result = await _paymentService.GetByOrderIdAsync(orderId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var result = await _paymentService.GetByCustomerIdAsync(customerId);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _paymentService.GetAllAsync();
        return Ok(result);
    }

    [HttpPost("order/{orderId}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(Guid orderId)
    {
        try
        {
            var result = await _paymentService.RefundAsync(orderId);
            if (result == null)
                return NotFound(new { message = "No payment record found for this order. The order may have been placed before the payment service was active." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
