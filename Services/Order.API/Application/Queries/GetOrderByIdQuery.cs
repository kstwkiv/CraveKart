using Order.API.Application.DTOs;

namespace Order.API.Application.Queries;

/// <summary>
/// Query to retrieve a single order by its unique identifier.
/// </summary>
/// <param name="OrderId">The unique identifier of the order to retrieve.</param>
public record GetOrderByIdQuery(Guid OrderId);
