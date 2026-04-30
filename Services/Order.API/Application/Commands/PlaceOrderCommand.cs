using Order.API.Application.DTOs;
using Order.API.Domain.Enums;

namespace Order.API.Application.Commands;

/// <summary>
/// Command to place a new food order. Calculates totals, persists the order,
/// and publishes an <c>OrderPlacedEvent</c> to trigger payment processing.
/// </summary>
/// <param name="CustomerId">The unique identifier of the customer placing the order.</param>
/// <param name="CustomerEmail">The customer's email address for order notifications.</param>
/// <param name="RestaurantId">The unique identifier of the restaurant being ordered from.</param>
/// <param name="RestaurantName">The display name of the restaurant.</param>
/// <param name="RestaurantLogoUrl">The URL of the restaurant's logo image.</param>
/// <param name="DeliveryAddress">The full delivery address for the order.</param>
/// <param name="PaymentMethod">The payment method selected by the customer.</param>
/// <param name="Items">The list of menu items included in the order.</param>
public record PlaceOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    Guid RestaurantId,
    string RestaurantName,
    string RestaurantLogoUrl,
    string DeliveryAddress,
    PaymentMethod PaymentMethod,
    List<OrderItemDto> Items);
