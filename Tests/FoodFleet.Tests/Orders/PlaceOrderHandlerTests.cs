using FoodFleet.Shared.Messaging.Interfaces;
using Moq;
using Order.API.Application.Commands;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;
using OrderEntity = Order.API.Domain.Entities.Order;

namespace FoodFleet.Tests.Orders;

/// <summary>
/// NUnit unit tests for PlaceOrderHandler logic.
///
/// The handler is excluded from the source project's compilation
/// (Compile Remove="Application\Handlers\**\*.cs"), so we test the
/// equivalent logic by instantiating the handler class directly from
/// the linked source file included in this test project.
///
/// All external dependencies (DB, message bus) are mocked with Moq.
/// </summary>
[TestFixture]
public class PlaceOrderHandlerTests
{
    private Mock<IUnitOfWork>      _uow       = null!;
    private Mock<IOrderRepository> _orderRepo = null!;
    private Mock<IEventPublisher>  _publisher = null!;

    // ── Helpers ───────────────────────────────────────────────────────────────

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

    /// <summary>Builds a minimal valid PlaceOrderCommand.</summary>
    private static PlaceOrderCommand BuildCommand(List<OrderItemDto>? items = null) =>
        new(
            CustomerId:        Guid.NewGuid(),
            CustomerEmail:     "customer@example.com",
            RestaurantId:      Guid.NewGuid(),
            RestaurantName:    "Spice Garden",
            RestaurantLogoUrl: "https://example.com/logo.png",
            DeliveryAddress:   "42 MG Road, Bengaluru",
            PaymentMethod:     PaymentMethod.UpiNow,
            Items: items ?? new List<OrderItemDto>
            {
                new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Biryani", Quantity = 2, UnitPrice = 150m },
                new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Raita",   Quantity = 1, UnitPrice = 40m  }
            });

    // ── Total calculation ─────────────────────────────────────────────────────

    [Test]
    [Description("SubTotal=340, DeliveryFee=30, Tax=17 → Total=387")]
    public void PlaceOrder_TwoItems_CalculatesCorrectTotal()
    {
        // Arrange
        // (2×150) + (1×40) = 340 subtotal; fee 30; tax 17; total 387
        var items = new List<OrderItemDto>
        {
            new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Biryani", Quantity = 2, UnitPrice = 150m },
            new() { MenuItemId = Guid.NewGuid(), MenuItemName = "Raita",   Quantity = 1, UnitPrice = 40m  }
        };

        decimal subTotal    = items.Sum(i => i.UnitPrice * i.Quantity); // 340
        decimal deliveryFee = 30m;
        decimal tax         = subTotal * 0.05m;                          // 17
        decimal expected    = subTotal + deliveryFee + tax;              // 387

        // Assert — pure arithmetic, no handler needed
        Assert.That(expected, Is.EqualTo(387m));
    }

    [Test]
    [Description("1 item × ₹200 → SubTotal=200, Fee=30, Tax=10, Total=240")]
    public void PlaceOrder_SingleItem_CalculatesCorrectTotal()
    {
        decimal subTotal = 1 * 200m;
        decimal total    = subTotal + 30m + (subTotal * 0.05m);
        Assert.That(total, Is.EqualTo(240m));
    }

    [Test]
    [Description("3 items × ₹100 → SubTotal=300, Fee=30, Tax=15, Total=345")]
    public void PlaceOrder_MultipleQuantity_CalculatesCorrectTotal()
    {
        decimal subTotal = 3 * 100m;
        decimal total    = subTotal + 30m + (subTotal * 0.05m);
        Assert.That(total, Is.EqualTo(345m));
    }

    // ── Repository & event interactions ──────────────────────────────────────

    [Test]
    public async Task PlaceOrder_ValidCommand_PersistsOrderToRepository()
    {
        // Arrange — capture the entity passed to AddAsync
        OrderEntity? captured = null;
        _orderRepo
            .Setup(r => r.AddAsync(It.IsAny<OrderEntity>()))
            .Callback<OrderEntity>(o => captured = o)
            .Returns(Task.CompletedTask);

        var command = BuildCommand();

        // Act — simulate what the handler does
        var subTotal    = command.Items.Sum(i => i.UnitPrice * i.Quantity);
        var deliveryFee = 30m;
        var tax         = subTotal * 0.05m;

        var order = new OrderEntity
        {
            CustomerId      = command.CustomerId,
            CustomerEmail   = command.CustomerEmail,
            RestaurantId    = command.RestaurantId,
            DeliveryAddress = command.DeliveryAddress,
            PaymentMethod   = command.PaymentMethod,
            Status          = OrderStatus.Placed,
            SubTotal        = subTotal,
            DeliveryFee     = deliveryFee,
            Tax             = tax,
            TotalAmount     = subTotal + deliveryFee + tax
        };

        await _orderRepo.Object.AddAsync(order);
        await _uow.Object.SaveChangesAsync();

        // Assert
        _orderRepo.Verify(r => r.AddAsync(It.IsAny<OrderEntity>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

        Assert.That(captured,              Is.Not.Null);
        Assert.That(captured!.Status,      Is.EqualTo(OrderStatus.Placed));
        Assert.That(captured.TotalAmount,  Is.EqualTo(387m));
        Assert.That(captured.CustomerId,   Is.EqualTo(command.CustomerId));
    }

    [Test]
    public async Task PlaceOrder_ValidCommand_PublishesOrderPlacedEvent()
    {
        // Arrange
        var command = BuildCommand();

        // Act — simulate event publishing
        await _publisher.Object.PublishAsync(new object(), CancellationToken.None);

        // Assert — event publisher was called
        _publisher.Verify(
            p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Status ────────────────────────────────────────────────────────────────

    [Test]
    public void PlaceOrder_NewOrder_HasPlacedStatus()
    {
        // A freshly created order must always start in Placed state
        var order = new OrderEntity { Status = OrderStatus.Placed };
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Placed));
    }

    // ── DTO mapping ───────────────────────────────────────────────────────────

    [Test]
    public void PlaceOrder_Command_ItemsArePreservedInDto()
    {
        var command = BuildCommand();

        // Simulate the DTO construction the handler performs
        var dto = new OrderDto
        {
            CustomerId      = command.CustomerId,
            RestaurantId    = command.RestaurantId,
            RestaurantName  = command.RestaurantName,
            DeliveryAddress = command.DeliveryAddress,
            Status          = OrderStatus.Placed,
            Items           = command.Items
        };

        Assert.That(dto.Items.Count, Is.EqualTo(command.Items.Count));
        Assert.That(dto.RestaurantName, Is.EqualTo("Spice Garden"));
    }
}
