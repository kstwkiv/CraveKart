using Order.API.Domain.Enums;

namespace Order.API.Application.Commands;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus);
