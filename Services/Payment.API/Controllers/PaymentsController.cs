using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.API.Application.Commands;
using Payment.API.Application.DTOs;
using Payment.API.Application.Interfaces;

namespace Payment.API.Controllers;

/// <summary>
/// API controller for payment processing operations.
/// Provides an endpoint for customers and admins to manually process payments.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentsController"/>.
    /// </summary>
    /// <param name="paymentService">The payment service for processing payments.</param>
    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Processes a payment for an order. The customer ID is extracted from the JWT token.
    /// </summary>
    /// <param name="request">The payment request containing order ID, amount, and payment method.</param>
    /// <returns>The created payment DTO.</returns>
    [HttpPost("process")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
    {
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.ProcessAsync(new ProcessPaymentCommand(
            request.OrderId,
            customerId,
            request.Amount,
            request.PaymentMethod));

        return Ok(result);
    }
}
