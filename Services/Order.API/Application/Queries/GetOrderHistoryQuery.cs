using Order.API.Application.DTOs;

namespace Order.API.Application.Queries;

/// <summary>
/// Query to retrieve the order history for a specific customer.
/// </summary>
/// <param name="CustomerId">The unique identifier of the customer whose orders to retrieve.</param>
public record GetOrderHistoryQuery(Guid CustomerId);
