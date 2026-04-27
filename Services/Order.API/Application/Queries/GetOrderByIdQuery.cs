using Order.API.Application.DTOs;

namespace Order.API.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId);
