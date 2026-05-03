// 'using' — imports a namespace so its types are available without full qualification
using FoodFleet.Shared.Events.Payments;
// 'using' — brings in the shared messaging interfaces (IEventPublisher)
using FoodFleet.Shared.Messaging.Interfaces;
// 'using' — imports the command object that carries payment request data
using Payment.API.Application.Commands;
// 'using' — imports the DTO (Data Transfer Object) types returned to callers
using Payment.API.Application.DTOs;
// 'using' — imports the IPaymentService contract this class implements
using Payment.API.Application.Interfaces;
// 'using' — imports the PaymentStatus enum used to express payment state
using Payment.API.Domain.Enums;

// 'namespace' — declares a logical grouping/scope for this class, preventing name collisions
namespace Payment.API.Application.Services;

/// <summary>
/// Application service implementing <see cref="IPaymentService"/> for core payment processing operations.
/// Handles UPI and COD payment flows, refunds, and publishes domain events via <see cref="IEventPublisher"/>.
/// </summary>
// 'public' — access modifier: this class is visible to any other assembly or namespace
// 'class' — defines a reference type that bundles state (fields) and behaviour (methods)
// 'IPaymentService' after ':' — interface implementation contract; this class must fulfil every member declared in IPaymentService
public class PaymentService : IPaymentService
{
    // 'private' — access modifier: field is only accessible within this class
    // 'readonly' — the field can only be assigned in the constructor; prevents accidental reassignment later
    private readonly IUnitOfWork _unitOfWork;
    // 'private readonly' — same pattern: injected dependency stored immutably
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="PaymentService"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    // Constructor — special method called when 'new PaymentService(...)' is invoked; used here for Dependency Injection
    public PaymentService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        // Assigning the injected dependency to the readonly backing field
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    // 'public' — method is accessible from outside the class
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<PaymentDto>' — Task is the .NET type representing an in-progress async operation; <PaymentDto> is the eventual result type
    // 'ProcessPaymentCommand' — a command object (CQRS pattern) carrying all data needed to process a payment
    // 'CancellationToken' — a cooperative cancellation primitive; callers can signal that the operation should stop
    public async Task<PaymentDto> ProcessAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default)
    {
        // 'var' — implicitly typed local variable; the compiler infers the type (Domain.Entities.Payment) from the right-hand side
        // 'new' — allocates a new instance of the specified type on the managed heap
        var payment = new Domain.Entities.Payment
        {
            // Object initialiser syntax — sets properties immediately after construction without separate assignment statements
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            // Enum member access — PaymentStatus.Confirmed is a named constant from the PaymentStatus enum
            Status = PaymentStatus.Confirmed,
            ProcessedAt = IstClock.Now
        };

        // 'await' — suspends execution of this async method until the awaited Task completes, freeing the thread in the meantime
        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Publishing a domain event so other services (e.g. Notification) can react
        await _eventPublisher.PublishAsync(new PaymentConfirmedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ConfirmedAt = IstClock.Now
        }, cancellationToken);

        // 'return' — exits the method and hands the value back to the caller
        return ToDto(payment);
    }

    // 'async Task<PaymentDto>' — same async pattern; creates a payment in Pending state without publishing a confirmed event
    public async Task<PaymentDto> CreatePendingAsync(ProcessPaymentCommand request, CancellationToken cancellationToken = default)
    {
        var payment = new Domain.Entities.Payment
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            // PaymentStatus.Pending — enum value indicating the payment is not yet settled (used for COD)
            Status = PaymentStatus.Pending,
            ProcessedAt = IstClock.Now
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(payment);
    }

    // 'Guid' — a 128-bit globally unique identifier type; used as a primary key to avoid collisions across distributed services
    // 'PaymentDto?' — the '?' makes the return type nullable, meaning the method may return null if no payment is found
    public async Task<PaymentDto?> ConfirmCodAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        // 'if' — conditional branch; executes the block only when the condition is true
        // 'null' — the absence of an object reference; checking for null guards against NullReferenceException
        // 'return' with null — early exit returning no value (caller receives null)
        if (payment == null) return null;
        if (payment.Status != PaymentStatus.Pending) return ToDto(payment);

        payment.Status = PaymentStatus.Confirmed;
        payment.ProcessedAt = IstClock.Now;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentConfirmedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ConfirmedAt = IstClock.Now
        }, cancellationToken);

        return ToDto(payment);
    }

    // Nullable return type 'PaymentDto?' — caller must handle the case where no payment exists for the given orderId
    public async Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        // Ternary operator — concise if/else: if payment is null return null, otherwise map to DTO
        return payment == null ? null : ToDto(payment);
    }

    // 'List<PaymentDto>' — a generic ordered collection from System.Collections.Generic; <PaymentDto> is the type parameter
    public async Task<List<PaymentDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetByCustomerIdAsync(customerId);
        // LINQ .Select() projects each Payment entity to a PaymentDto; .ToList() materialises the lazy sequence into a List<T>
        return payments.Select(ToDto).ToList();
    }

    public async Task<List<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetAllAsync();
        return payments.Select(ToDto).ToList();
    }

    public async Task<PaymentDto?> RefundAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);

        // 'if' + 'null' check — defensive guard; if no payment record exists, return null so the controller can respond with 404
        if (payment == null)
        {
            // Payment record missing (e.g. order placed before payment service was running).
            // Return null so the controller can return a clear 404.
            return null;
        }

        // 'throw' — raises an exception, unwinding the call stack until a matching catch block handles it
        // 'new InvalidOperationException(...)' — creates an exception object describing the illegal state
        if (payment.Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("This payment has already been refunded.");

        if (payment.Status == PaymentStatus.Failed)
            throw new InvalidOperationException("Cannot refund a failed payment.");

        payment.Status = PaymentStatus.Refunded;
        payment.ProcessedAt = IstClock.Now;
        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        await _eventPublisher.PublishAsync(new PaymentRefundedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            CustomerEmail = payment.CustomerEmail,
            Amount = payment.Amount,
            RefundedAt = IstClock.Now
        }, cancellationToken);

        return ToDto(payment);
    }

    // 'private' — helper method not exposed outside this class
    // 'static' — belongs to the type itself, not to any instance; no 'this' reference needed
    // Expression-bodied member (=>) — concise single-expression method body
    private static PaymentDto ToDto(Domain.Entities.Payment p) => new()
    {
        // Object initialiser mapping entity fields to DTO properties
        Id = p.Id,
        OrderId = p.OrderId,
        CustomerId = p.CustomerId,
        Amount = p.Amount,
        Currency = p.Currency,
        // .ToString() — converts the enum value to its string name (e.g. "Confirmed")
        Status = p.Status.ToString(),
        PaymentMethod = p.PaymentMethod,
        CreatedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt
    };
}
