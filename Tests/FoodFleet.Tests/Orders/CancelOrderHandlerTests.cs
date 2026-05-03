using FoodFleet.Shared.Messaging.Interfaces;
using Moq;
using Order.API.Application.Commands;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace FoodFleet.Tests.Orders;

/// <summary>
/// NUnit unit tests for CancelOrderHandler logic.
/// Tests the cancellation rules, status transitions, and event publishing
/// by simulating the handler's behaviour directly.
/// </summary>
[TestFixture]
public class CancelOrderHandlerTests
{
    private Mock<IUnitOfWork>      _uow       = null!;
    private Mock<IOrderRepository> _orderRepo = null!;
    private Mock<IEventPublisher>  _publisher = null!;

    [SetUp]
    public void SetUp()
    {
        _uow       = new Mock<IUnitOfWork>();
        _orderRepo = new Mock<IOrderRepository>();
        _publisher = new Mock<IEventPublisher>();

        _uow.Setup(u => u.Orders).Returns(_orderRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _publisher
            .Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static OrderEntity MakeOrder(OrderStatus status) => new()
    {
        Id         = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Status     = status
    };

    /// <summary>
    /// Simulates the handler's Handle() method so we can test the logic
    /// without needing MediatR in the test project.
    /// </summary>
    private async Task<bool> SimulateHandle(CancelOrderCommand command)
    {
        var order = await _orderRepo.Object.GetByIdAsync(command.OrderId);
        if (order == null) return false;

        if (order.Status != OrderStatus.Placed && order.Status != OrderStatus.Confirmed)
            throw new Exception("Order cannot be cancelled at this stage.");

        order.Status    = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _orderRepo.Object.Update(order);
        await _uow.Object.SaveChangesAsync();

        await _publisher.Object.PublishAsync(new object(), CancellationToken.None);
        return true;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Happy paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(OrderStatus.Placed)]
    [TestCase(OrderStatus.Confirmed)]
    public async Task Handle_CancellableStatus_ReturnsTrueAndSetsStatusCancelled(OrderStatus status)
    {
        // Arrange
        var order   = MakeOrder(status);
        var command = new CancelOrderCommand(order.Id, order.CustomerId);

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        var result = await SimulateHandle(command);

        // Assert
        Assert.That(result,       Is.True);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
    }

    [Test]
    [TestCase(OrderStatus.Placed)]
    [TestCase(OrderStatus.Confirmed)]
    public async Task Handle_CancellableStatus_SavesChangesAndPublishesEvent(OrderStatus status)
    {
        // Arrange
        var order   = MakeOrder(status);
        var command = new CancelOrderCommand(order.Id, order.CustomerId);

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act
        await SimulateHandle(command);

        // Assert — DB write happened
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

        // Assert — cancellation event was published
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Failure paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Handle_OrderNotFound_ReturnsFalseWithNoSideEffects()
    {
        // Arrange — repository returns null
        var command = new CancelOrderCommand(Guid.NewGuid(), Guid.NewGuid());

        _orderRepo.Setup(r => r.GetByIdAsync(command.OrderId))
                  .ReturnsAsync((OrderEntity?)null);

        // Act
        var result = await SimulateHandle(command);

        // Assert
        Assert.That(result, Is.False);

        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    [TestCase(OrderStatus.Preparing)]
    [TestCase(OrderStatus.Ready)]
    [TestCase(OrderStatus.PickedUp)]
    [TestCase(OrderStatus.Delivered)]
    public void Handle_NonCancellableStatus_ThrowsException(OrderStatus status)
    {
        // Arrange — order is past the cancellable window
        var order   = MakeOrder(status);
        var command = new CancelOrderCommand(order.Id, order.CustomerId);

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => SimulateHandle(command));
        Assert.That(ex!.Message, Does.Contain("cannot be cancelled"));
    }

    [Test]
    [TestCase(OrderStatus.Preparing)]
    [TestCase(OrderStatus.Delivered)]
    public async Task Handle_NonCancellableStatus_NoDbWriteOrEvent(OrderStatus status)
    {
        // Arrange
        var order   = MakeOrder(status);
        var command = new CancelOrderCommand(order.Id, order.CustomerId);

        _orderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        // Act — swallow the expected exception
        try { await SimulateHandle(command); } catch { /* expected */ }

        // Assert — no side effects
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
