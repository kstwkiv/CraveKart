using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;

namespace Restaurant.API.Application.Interfaces;

public interface IRestaurantService
{
    Task<List<RestaurantDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default);
    Task<RestaurantDto?> GetByIdAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<RestaurantDto> CreateAsync(CreateRestaurantCommand request, CancellationToken cancellationToken = default);
    Task<RestaurantDto?> UpdateAsync(Guid restaurantId, Guid requestingOwnerId, UpdateRestaurantCommand request, CancellationToken cancellationToken = default);
    Task<bool?> ToggleAvailabilityAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemCommand request, CancellationToken cancellationToken = default);
}
