using Delivery.API.Application.DTOs;

namespace Delivery.API.Application.Commands;

public record AssignDeliveryCommand(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail);
