// 'using' — imports System.Security.Claims so ClaimTypes and ClaimsPrincipal helpers are available
using System.Security.Claims;
// 'using' — imports ASP.NET Core's authorisation attributes ([Authorize], [AllowAnonymous])
using Microsoft.AspNetCore.Authorization;
// 'using' — imports ASP.NET Core MVC types: ControllerBase, IActionResult, route/HTTP-method attributes
using Microsoft.AspNetCore.Mvc;
// 'using' — imports the ProcessPaymentCommand used to carry validated request data into the service layer
using Payment.API.Application.Commands;
// 'using' — imports the DTO types returned in HTTP responses
using Payment.API.Application.DTOs;
// 'using' — imports the IPaymentService interface; the controller depends on the abstraction, not the concrete class
using Payment.API.Application.Interfaces;

// 'namespace' — scopes this controller to the Payment API's Controllers layer
namespace Payment.API.Controllers;

/// <summary>
/// API controller for payment processing operations.
/// Provides endpoints for processing payments, retrieving payment records, and issuing refunds.
/// </summary>
// [ApiController] — attribute that enables automatic model validation, binding-source inference, and problem-detail responses
// [Route(...)] — defines the URL template; [controller] is replaced by the class name minus "Controller" → "payments"
// [Authorize] — class-level attribute: every action requires an authenticated JWT bearer token by default
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
// 'public' — the class must be public for ASP.NET Core's routing middleware to discover it
// 'class' — reference type; inherits from ControllerBase which provides HTTP helpers (Ok, NotFound, BadRequest, etc.)
// ': ControllerBase' — base class providing action-result helpers without Razor view support
public class PaymentsController : ControllerBase
{
    // 'private readonly' — injected service stored immutably; only assigned in the constructor
    private readonly IPaymentService _paymentService;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentsController"/>.
    /// </summary>
    /// <param name="paymentService">The payment service for business logic.</param>
    // Constructor injection — ASP.NET Core's DI container resolves IPaymentService and passes it here
    // Expression-bodied constructor (=>) — concise single-assignment constructor body
    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    /// <summary>
    /// Processes a payment for an order. Accessible by Customer and Admin roles.
    /// </summary>
    /// <param name="request">The payment request containing order ID, amount, and payment method.</param>
    /// <returns>The created payment DTO.</returns>
    // [HttpPost("process")] — maps HTTP POST /api/v1/payments/process to this action
    // [Authorize(Roles = "...")] — overrides the class-level [Authorize]; restricts to specific roles
    [HttpPost("process")]
    [Authorize(Roles = "Customer,Admin")]
    // 'public' — action must be public for the MVC routing engine to invoke it
    // 'async' — enables awaiting async service calls without blocking a thread
    // 'Task<IActionResult>' — async return; IActionResult is the base type for all HTTP responses (Ok, NotFound, etc.)
    // '[FromBody]' — attribute telling the model binder to deserialise the request body as ProcessPaymentRequest
    public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
    {
        // 'var' — implicitly typed; Guid.Parse returns a Guid
        // User.FindFirstValue — reads a claim from the authenticated JWT; ClaimTypes.NameIdentifier is the user's ID claim
        // '!' — null-forgiving operator: asserts the value is non-null (safe here because [Authorize] guarantees authentication)
        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'await' — suspends until ProcessAsync completes; the thread is freed during the I/O wait
        var result = await _paymentService.ProcessAsync(new ProcessPaymentCommand(
            request.OrderId,
            customerId,
            // '??' — null-coalescing operator: uses string.Empty if CustomerEmail is null
            request.CustomerEmail ?? string.Empty,
            request.Amount,
            request.PaymentMethod));
        // Ok(result) — returns HTTP 200 with the result serialised as JSON
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the payment record for a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>The payment DTO, or 404 if not found.</returns>
    // [HttpGet("order/{orderId}")] — maps HTTP GET /api/v1/payments/order/{orderId}; {orderId} is a route parameter
    [HttpGet("order/{orderId}")]
    // 'Guid orderId' — route parameter automatically parsed from the URL segment into a Guid value type
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var result = await _paymentService.GetByOrderIdAsync(orderId);
        // 'if' — conditional: return 404 when no payment is found, otherwise 200 with the DTO
        // 'null' — absence of a value; the service returns null when no record matches
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all payment records for a specific customer. Accessible by Customer and Admin roles.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>A list of payment DTOs for the customer.</returns>
    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetByCustomer(Guid customerId)
    {
        var result = await _paymentService.GetByCustomerIdAsync(customerId);
        // Ok(result) — HTTP 200; an empty list is still a valid successful response
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all payment records. Accessible by Admin role only.
    /// </summary>
    /// <returns>A list of all payment DTOs.</returns>
    [HttpGet]
    // [Authorize(Roles = "Admin")] — restricts this endpoint to users whose JWT contains the "Admin" role claim
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _paymentService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Issues a refund for a confirmed payment associated with an order. Accessible by Admin role only.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order to refund.</param>
    /// <returns>The updated payment DTO, or 404/400 on failure.</returns>
    [HttpPost("order/{orderId}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(Guid orderId)
    {
        // 'try' — begins a guarded block; exceptions thrown inside are caught by the matching 'catch'
        try
        {
            var result = await _paymentService.RefundAsync(orderId);
            // 'if' + null check — guards against missing payment records
            if (result == null)
                // NotFound(new { ... }) — HTTP 404 with a JSON body describing the problem
                return NotFound(new { message = "No payment record found for this order. The order may have been placed before the payment service was active." });
            return Ok(result);
        }
        // 'catch (InvalidOperationException ex)' — catches only the specific exception type thrown by the service
        //   for illegal state transitions (already refunded, or failed payment)
        catch (InvalidOperationException ex)
        {
            // BadRequest — HTTP 400; the client sent a logically invalid request (e.g. refunding an already-refunded payment)
            return BadRequest(new { message = ex.Message });
        }
    }
}
