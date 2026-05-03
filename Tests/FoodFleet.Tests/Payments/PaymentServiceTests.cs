using FoodFleet.Shared.Messaging.Interfaces;
using Moq;
using Payment.API.Application.Commands;
using Payment.API.Application.Interfaces;
using Payment.API.Application.Services;
using Payment.API.Domain.Enums;
using PaymentEntity = Payment.API.Domain.Entities.Payment;

namespace FoodFleet.Tests.Payments;

/// <summary>
/// NUnit unit tests for <see cref="PaymentService"/>.
/// Covers UPI processing, COD pending creation, refund logic, and error cases.
/// </summary>
[TestFixture]
public class PaymentServiceTests
{
    private Mock<IUnitOfWork>        _uow         = null!;
    private Mock<IPaymentRepository> _paymentRepo = null!;
    private Mock<IEventPublisher>    _publisher   = null!;
    private PaymentService           _sut         = null!;

    [SetUp]
    public void SetUp()
    {
        _uow         = new Mock<IUnitOfWork>();
        _paymentRepo = new Mock<IPaymentRepository>();
        _publisher   = new Mock<IEventPublisher>();

        _uow.Setup(u => u.Payments).Returns(_paymentRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _publisher.Setup(p => p.PublishAsync(
                It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new PaymentService(_uow.Object, _publisher.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProcessPaymentCommand MakeCommand(string method = "UpiNow") =>
        new(
            OrderId:       Guid.NewGuid(),
            CustomerId:    Guid.NewGuid(),
            CustomerEmail: "customer@example.com",
            Amount:        500m,
            PaymentMethod: method);

    private static PaymentEntity MakePayment(
        Guid? orderId = null,
        PaymentStatus status = PaymentStatus.Confirmed) => new()
    {
        Id            = Guid.NewGuid(),
        OrderId       = orderId ?? Guid.NewGuid(),
        CustomerId    = Guid.NewGuid(),
        CustomerEmail = "customer@example.com",
        Amount        = 500m,
        Status        = status,
        PaymentMethod = "UpiNow"
    };

    // ═════════════════════════════════════════════════════════════════════════
    // ProcessAsync (UPI — immediate confirmation)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ProcessAsync_ValidCommand_ReturnsConfirmedPaymentDto()
    {
        // Arrange
        var command = MakeCommand("UpiNow");

        // Act
        var result = await _sut.ProcessAsync(command);

        // Assert — DTO reflects Confirmed status
        Assert.That(result,               Is.Not.Null);
        Assert.That(result.Status,        Is.EqualTo(PaymentStatus.Confirmed.ToString()));
        Assert.That(result.Amount,        Is.EqualTo(command.Amount));
        Assert.That(result.PaymentMethod, Is.EqualTo(command.PaymentMethod));
    }

    [Test]
    public async Task ProcessAsync_ValidCommand_PersistsAndPublishesConfirmedEvent()
    {
        // Arrange
        var command = MakeCommand();

        // Act
        await _sut.ProcessAsync(command);

        // Assert — payment was saved
        _paymentRepo.Verify(r => r.AddAsync(It.IsAny<PaymentEntity>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

        // Assert — PaymentConfirmedEvent was published
        _publisher.Verify(p => p.PublishAsync(
            It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // CreatePendingAsync (COD — deferred confirmation)
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task CreatePendingAsync_CodCommand_ReturnsPendingPaymentDto()
    {
        // Arrange
        var command = MakeCommand("CashOnDelivery");

        // Act
        var result = await _sut.CreatePendingAsync(command);

        // Assert — status must be Pending, not Confirmed
        Assert.That(result.Status, Is.EqualTo(PaymentStatus.Pending.ToString()));
    }

    [Test]
    public async Task CreatePendingAsync_CodCommand_DoesNotPublishEvent()
    {
        // Arrange
        var command = MakeCommand("CashOnDelivery");

        // Act
        await _sut.CreatePendingAsync(command);

        // Assert — no event should be published for a pending COD payment
        _publisher.Verify(p => p.PublishAsync(
            It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // RefundAsync
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task RefundAsync_ConfirmedPayment_ReturnsRefundedDtoAndPublishesEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = MakePayment(orderId, PaymentStatus.Confirmed);

        _paymentRepo.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(payment);

        // Act
        var result = await _sut.RefundAsync(orderId);

        // Assert
        Assert.That(result,        Is.Not.Null);
        Assert.That(result!.Status, Is.EqualTo(PaymentStatus.Refunded.ToString()));

        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _publisher.Verify(p => p.PublishAsync(
            It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RefundAsync_PaymentNotFound_ReturnsNull()
    {
        // Arrange — no payment record for this order
        var orderId = Guid.NewGuid();
        _paymentRepo.Setup(r => r.GetByOrderIdAsync(orderId))
                    .ReturnsAsync((PaymentEntity?)null);

        // Act
        var result = await _sut.RefundAsync(orderId);

        // Assert
        Assert.That(result, Is.Null);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void RefundAsync_AlreadyRefunded_ThrowsInvalidOperationException()
    {
        // Arrange — payment is already in Refunded state
        var orderId = Guid.NewGuid();
        var payment = MakePayment(orderId, PaymentStatus.Refunded);

        _paymentRepo.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(payment);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RefundAsync(orderId));

        Assert.That(ex!.Message, Does.Contain("already been refunded"));
    }

    [Test]
    public void RefundAsync_FailedPayment_ThrowsInvalidOperationException()
    {
        // Arrange — cannot refund a payment that never succeeded
        var orderId = Guid.NewGuid();
        var payment = MakePayment(orderId, PaymentStatus.Failed);

        _paymentRepo.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(payment);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RefundAsync(orderId));

        Assert.That(ex!.Message, Does.Contain("Cannot refund a failed payment"));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // GetByOrderIdAsync
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetByOrderIdAsync_ExistingOrder_ReturnsDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = MakePayment(orderId);

        _paymentRepo.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(payment);

        // Act
        var result = await _sut.GetByOrderIdAsync(orderId);

        // Assert
        Assert.That(result,          Is.Not.Null);
        Assert.That(result!.OrderId, Is.EqualTo(orderId));
    }

    [Test]
    public async Task GetByOrderIdAsync_MissingOrder_ReturnsNull()
    {
        // Arrange
        _paymentRepo.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((PaymentEntity?)null);

        // Act
        var result = await _sut.GetByOrderIdAsync(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Null);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // GetByCustomerIdAsync
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetByCustomerIdAsync_ReturnsAllCustomerPayments()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var payments   = new List<PaymentEntity>
        {
            MakePayment(), MakePayment(), MakePayment()
        };

        _paymentRepo.Setup(r => r.GetByCustomerIdAsync(customerId)).ReturnsAsync(payments);

        // Act
        var result = await _sut.GetByCustomerIdAsync(customerId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }
}
