using Order.API.Application.Commands;
using Order.API.Application.DTOs;

namespace Order.API.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<OrderDto>> GetHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(CancelOrderCommand request, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetByRestaurantAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetByStatusAsync(Order.API.Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}
