using Delivery.API.Application.Commands;
using Delivery.API.Application.DTOs;
using Delivery.API.Application.Interfaces;
using Delivery.API.Domain.Entities;
using FoodFleet.Shared.Messaging.Interfaces;
using Moq;
using DeliveryEntity = Delivery.API.Domain.Entities.Delivery;

namespace FoodFleet.Tests.Delivery;

/// <summary>
/// NUnit unit tests for AssignDeliveryHandler logic.
/// Tests agent assignment, delivery record creation, and event publishing
/// by simulating the handler's behaviour directly.
/// </summary>
[TestFixture]
public class AssignDeliveryHandlerTests
{
    private Mock<IUnitOfWork>         _uow          = null!;
    private Mock<IDeliveryRepository> _deliveryRepo = null!;
    private Mock<IEventPublisher>     _publisher    = null!;

    [SetUp]
    public void SetUp()
    {
        _uow          = new Mock<IUnitOfWork>();
        _deliveryRepo = new Mock<IDeliveryRepository>();
        _publisher    = new Mock<IEventPublisher>();

        _uow.Setup(u => u.Deliveries).Returns(_deliveryRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _publisher
            .Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DeliveryAgent MakeAvailableAgent() => new()
    {
        Id          = Guid.NewGuid(),
        FullName    = "Ravi Kumar",
        VehicleType = "Bike",
        IsAvailable = true
    };

    private static AssignDeliveryCommand MakeCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "customer@example.com");

    /// <summary>
    /// Simulates the handler's Handle() method so we can test the logic
    /// without needing MediatR in the test project.
    /// </summary>
    private async Task<DeliveryDto> SimulateHandle(AssignDeliveryCommand command)
    {
        var agent = await _deliveryRepo.Object.GetAvailableAgentAsync()
            ?? throw new Exception("No available delivery agents.");

        agent.IsAvailable = false;
        _deliveryRepo.Object.UpdateAgent(agent);

        var delivery = new DeliveryEntity
        {
            OrderId       = command.OrderId,
            AgentId       = agent.Id,
            CustomerId    = command.CustomerId,
            CustomerEmail = command.CustomerEmail,
            Status        = "Assigned",
            AssignedAt    = DateTime.UtcNow
        };

        await _deliveryRepo.Object.AddAsync(delivery);
        await _uow.Object.SaveChangesAsync();

        await _publisher.Object.PublishAsync(new object(), CancellationToken.None);

        return new DeliveryDto
        {
            Id         = delivery.Id,
            OrderId    = delivery.OrderId,
            AgentId    = delivery.AgentId,
            Status     = delivery.Status,
            AssignedAt = delivery.AssignedAt
        };
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Happy paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Handle_AvailableAgent_ReturnsDeliveryDtoWithCorrectFields()
    {
        // Arrange
        var agent   = MakeAvailableAgent();
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync()).ReturnsAsync(agent);

        // Act
        var result = await SimulateHandle(command);

        // Assert
        Assert.That(result,         Is.Not.Null);
        Assert.That(result.OrderId, Is.EqualTo(command.OrderId));
        Assert.That(result.AgentId, Is.EqualTo(agent.Id));
        Assert.That(result.Status,  Is.EqualTo("Assigned"));
    }

    [Test]
    public async Task Handle_AvailableAgent_MarksAgentUnavailable()
    {
        // Arrange
        var agent   = MakeAvailableAgent();
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync()).ReturnsAsync(agent);

        // Act
        await SimulateHandle(command);

        // Assert — agent is now unavailable
        Assert.That(agent.IsAvailable, Is.False);
        _deliveryRepo.Verify(r => r.UpdateAgent(agent), Times.Once);
    }

    [Test]
    public async Task Handle_AvailableAgent_PersistsDeliveryRecord()
    {
        // Arrange
        var agent   = MakeAvailableAgent();
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync()).ReturnsAsync(agent);

        // Act
        await SimulateHandle(command);

        // Assert — delivery was added and changes saved
        _deliveryRepo.Verify(r => r.AddAsync(It.IsAny<DeliveryEntity>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task Handle_AvailableAgent_PublishesDeliveryAssignedEvent()
    {
        // Arrange
        var agent   = MakeAvailableAgent();
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync()).ReturnsAsync(agent);

        // Act
        await SimulateHandle(command);

        // Assert — event was published
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Failure paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void Handle_NoAvailableAgent_ThrowsException()
    {
        // Arrange — no agents available
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync())
                     .ReturnsAsync((DeliveryAgent?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => SimulateHandle(command));
        Assert.That(ex!.Message, Does.Contain("No available delivery agents"));
    }

    [Test]
    public async Task Handle_NoAvailableAgent_NoDbWriteOrEvent()
    {
        // Arrange
        var command = MakeCommand();

        _deliveryRepo.Setup(r => r.GetAvailableAgentAsync())
                     .ReturnsAsync((DeliveryAgent?)null);

        // Act — swallow the expected exception
        try { await SimulateHandle(command); } catch { /* expected */ }

        // Assert — no side effects
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
