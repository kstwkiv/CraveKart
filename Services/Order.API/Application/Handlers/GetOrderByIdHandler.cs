using MediatR;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Application.Queries;

namespace Order.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="GetOrderByIdQuery"/> requests.
/// Retrieves a single order by its unique identifier and maps it to an <see cref="OrderDto"/>.
/// </summary>
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="GetOrderByIdHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public GetOrderByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query by fetching the order and mapping it to a DTO.
    /// </summary>
    /// <param name="request">The query containing the order ID to look up.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="OrderDto"/> if found; otherwise <c>null</c>.</returns>
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null) return null;

        return new OrderDto
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
                UnitPrice = i.UnitPrice,
                Customizations = i.Customizations
            }).ToList()
        };
    }
}