using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Application.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IUnitOfWork _unitOfWork;

    public RestaurantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RestaurantDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        var restaurants = string.IsNullOrEmpty(searchTerm)
            ? await _unitOfWork.Restaurants.GetAllActiveAsync()
            : await _unitOfWork.Restaurants.SearchAsync(searchTerm);

        return restaurants.Select(ToRestaurantDto).ToList();
    }

    public async Task<RestaurantDto?> GetByIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        return restaurant == null ? null : ToRestaurantDto(restaurant);
    }

    public async Task<RestaurantDto> CreateAsync(CreateRestaurantCommand request, CancellationToken cancellationToken = default)
    {
        var restaurant = new Domain.Entities.Restaurant
        {
            OwnerId = request.OwnerId,
            OwnerEmail = request.OwnerEmail,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Lat = request.Lat,
            Lng = request.Lng,
            CuisineTypes = request.CuisineTypes,
            OperatingHours = request.OperatingHours,
            MinimumOrderAmount = request.MinimumOrderAmount,
            EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes,
            Status = RestaurantStatus.Pending,
            LogoUrl = request.LogoUrl
        };

        await _unitOfWork.Restaurants.AddAsync(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return ToRestaurantDto(restaurant);
    }

    public async Task<RestaurantDto?> UpdateAsync(Guid restaurantId, Guid requestingOwnerId, UpdateRestaurantCommand request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        if (restaurant == null) return null;
        if (restaurant.OwnerId != requestingOwnerId) return null; // ownership check

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.Address = request.Address;
        restaurant.CuisineTypes = request.CuisineTypes;
        restaurant.OperatingHours = request.OperatingHours;
        restaurant.MinimumOrderAmount = request.MinimumOrderAmount;
        restaurant.EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes;
        if (request.LogoUrl != null) restaurant.LogoUrl = request.LogoUrl;
        restaurant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return ToRestaurantDto(restaurant);
    }

    public async Task<bool?> ToggleAvailabilityAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        if (restaurant == null) return null;

        restaurant.IsOpen = !restaurant.IsOpen;
        restaurant.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return restaurant.IsOpen;
    }

    public async Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Menus.GetCategoriesWithItemsAsync(restaurantId);

        return categories
            .SelectMany(c => c.MenuItems)
            .Select(ToMenuItemDto)
            .ToList();
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemCommand request, CancellationToken cancellationToken = default)
    {
        var item = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DietaryTags = request.DietaryTags,
            IsAvailable = true,
            ImageUrl = request.ImageUrl
        };

        await _unitOfWork.Menus.AddItemAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return ToMenuItemDto(item);
    }

    private static RestaurantDto ToRestaurantDto(Domain.Entities.Restaurant restaurant) => new()
    {
        Id = restaurant.Id,
        OwnerId = restaurant.OwnerId,
        Name = restaurant.Name,
        Description = restaurant.Description,
        Address = restaurant.Address,
        CuisineTypes = restaurant.CuisineTypes,
        OperatingHours = restaurant.OperatingHours,
        AverageRating = restaurant.AverageRating,
        TotalReviews = restaurant.TotalReviews,
        IsOpen = restaurant.IsOpen,
        EstimatedDeliveryMinutes = restaurant.EstimatedDeliveryMinutes,
        MinimumOrderAmount = restaurant.MinimumOrderAmount,
        Status = restaurant.Status.ToString(),
        LogoUrl = restaurant.LogoUrl
    };

    private static MenuItemDto ToMenuItemDto(MenuItem item) => new()
    {
        Id = item.Id,
        CategoryId = item.CategoryId,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        ImageUrl = item.ImageUrl,
        IsAvailable = item.IsAvailable,
        DietaryTags = item.DietaryTags
    };
}
