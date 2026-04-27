using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.API.Application.Commands;
using Payment.API.Application.Interfaces;

namespace Payment.API.Infrastructure.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(IPaymentService paymentService, ILogger<OrderPlacedConsumer> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        _logger.LogInformation("Processing payment for Order {OrderId}", context.Message.OrderId);

        await _paymentService.ProcessAsync(new ProcessPaymentCommand(
            context.Message.OrderId,
            context.Message.CustomerId,
            context.Message.TotalAmount,
            context.Message.PaymentMethod),
            context.CancellationToken);
    }
}
