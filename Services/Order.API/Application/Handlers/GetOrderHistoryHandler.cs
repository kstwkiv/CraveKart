using MediatR;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Application.Queries;

namespace Order.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="GetOrderHistoryQuery"/> requests.
/// Retrieves all orders for a specific customer and maps them to <see cref="OrderDto"/> records.
/// </summary>
public class GetOrderHistoryHandler : IRequestHandler<GetOrderHistoryQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="GetOrderHistoryHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public GetOrderHistoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query by fetching the customer's order history and mapping each order to a DTO.
    /// </summary>
    /// <param name="request">The query containing the customer ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="OrderDto"/> records for the customer.</returns>
    public async Task<List<OrderDto>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(request.CustomerId);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            RestaurantId = order.RestaurantId,
            DeliveryAddress = order.DeliveryAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItemName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        }).ToList();
    }
}