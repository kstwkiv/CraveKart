namespace Order.API.Application.Commands;

public record CancelOrderCommand(Guid OrderId, Guid CustomerId);
