namespace Delivery.API.Application.Commands;

public record CompleteDeliveryCommand(Guid OrderId);
