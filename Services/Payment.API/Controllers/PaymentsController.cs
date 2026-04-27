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

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

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
