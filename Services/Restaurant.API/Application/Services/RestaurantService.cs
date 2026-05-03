// 'using' — imports a namespace so its types are available without full qualification
using Restaurant.API.Application.Commands;
// 'using' — brings in the DTOs namespace (Data Transfer Objects used to shape API responses)
using Restaurant.API.Application.DTOs;
// 'using' — imports the Interfaces namespace so IRestaurantService and IUnitOfWork are in scope
using Restaurant.API.Application.Interfaces;
// 'using' — imports the Domain Entities namespace (core business objects)
using Restaurant.API.Domain.Entities;
// 'using' — imports the Domain Enums namespace (strongly-typed constant sets like RestaurantStatus)
using Restaurant.API.Domain.Enums;

// 'namespace' — declares a logical container that groups related types and avoids name collisions
namespace Restaurant.API.Application.Services;

// 'public' — access modifier: this class is visible to any other code in any assembly
// 'class' — defines a reference type that bundles state (fields) and behaviour (methods)
// ':' after the class name — inheritance/implementation syntax; RestaurantService implements IRestaurantService
public class RestaurantService : IRestaurantService
{
    // 'private' — access modifier: this field is only accessible within this class
    // 'readonly' — the field can only be assigned in the constructor; enforces immutability after construction
    private readonly IUnitOfWork _unitOfWork;

    // 'public' — constructor is accessible so the DI container can call it to inject dependencies
    public RestaurantService(IUnitOfWork unitOfWork)
    {
        // Assigns the injected dependency to the readonly field (constructor injection pattern)
        _unitOfWork = unitOfWork;
    }

    // 'public' — method is part of the public API surface (required by the interface)
    // 'async' — marks the method as asynchronous; the compiler rewrites it as a state machine
    // 'Task<T>' — represents an in-flight asynchronous operation that will eventually produce a value of type T
    // 'List<T>' — a generic, ordered, resizable collection from System.Collections.Generic
    // 'string' — built-in reference type for immutable sequences of Unicode characters
    // '?' after string — nullable annotation: the parameter may be null (no value provided)
    public async Task<List<RestaurantDto>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        // 'var' — implicitly typed local variable; the compiler infers the type from the right-hand side
        // 'await' — suspends execution of this method until the awaited Task completes, freeing the thread
        var restaurants = string.IsNullOrEmpty(searchTerm)
            ? await _unitOfWork.Restaurants.GetAllActiveAsync()
            : await _unitOfWork.Restaurants.SearchAsync(searchTerm);

        // 'return' — exits the method and hands the computed value back to the caller
        return restaurants.Select(ToRestaurantDto).ToList();
    }

    // 'Guid' — a 128-bit globally unique identifier; used as a collision-resistant primary key
    // '?' after RestaurantDto — nullable return type: the method may return null if not found
    public async Task<RestaurantDto?> GetByIdAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        // 'null' — the absence of a value; used here to signal "not found"
        // '==' — equality operator; for reference types checks reference equality unless overridden
        return restaurant == null ? null : ToRestaurantDto(restaurant);
    }

    public async Task<RestaurantDto> CreateAsync(CreateRestaurantCommand request, CancellationToken cancellationToken = default)
    {
        // 'new' — allocates a new instance of the specified type on the managed heap
        // 'Domain.Entities.Restaurant' — fully qualified type name used to disambiguate from the service class
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
            // 'RestaurantStatus.Pending' — enum member access; enums give named, type-safe integer constants
            Status = RestaurantStatus.Pending,
            LogoUrl = request.LogoUrl
        };

        // 'await' — waits for the async persistence operation before continuing
        await _unitOfWork.Restaurants.AddAsync(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return ToRestaurantDto(restaurant);
    }

    public async Task<RestaurantDto?> UpdateAsync(Guid restaurantId, Guid requestingOwnerId, UpdateRestaurantCommand request, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        // 'if' — conditional control-flow: executes the block only when the condition is true
        // 'return' used here as an early-exit guard clause (returns null when not found)
        if (restaurant == null) return null;
        if (restaurant.OwnerId != requestingOwnerId) return null;

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.Address = request.Address;
        restaurant.CuisineTypes = request.CuisineTypes;
        restaurant.OperatingHours = request.OperatingHours;
        restaurant.MinimumOrderAmount = request.MinimumOrderAmount;
        restaurant.EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes;
        // 'if' — guard: only update LogoUrl when a new value is explicitly provided (not null)
        // 'null' — checked here to avoid overwriting an existing logo with nothing
        if (request.LogoUrl != null) restaurant.LogoUrl = request.LogoUrl;
        restaurant.UpdatedAt = IstClock.Now;

        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return ToRestaurantDto(restaurant);
    }

    // 'bool' — built-in value type representing a true/false Boolean value
    // 'bool?' — nullable bool: can be true, false, or null (null = restaurant not found)
    public async Task<bool?> ToggleAvailabilityAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);
        // 'null' — sentinel value indicating the restaurant does not exist
        if (restaurant == null) return null;

        // '!' — logical NOT operator; flips the boolean value (toggle pattern)
        restaurant.IsOpen = !restaurant.IsOpen;
        restaurant.UpdatedAt = IstClock.Now;
        _unitOfWork.Restaurants.Update(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return restaurant.IsOpen;
    }

    public async Task<List<MenuItemDto>> GetMenuAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Menus.GetCategoriesWithItemsAsync(restaurantId);

        return categories
            .SelectMany(c => c.MenuItems) // SelectMany flattens nested collections into a single sequence
            .Select(ToMenuItemDto)
            .ToList();
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemCommand request, CancellationToken cancellationToken = default)
    {
        // 'new' — creates a new MenuItem instance; object initializer sets properties inline
        var item = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DietaryTags = request.DietaryTags,
            // 'true' — boolean literal; item is available immediately upon creation
            IsAvailable = true,
            ImageUrl = request.ImageUrl
        };

        await _unitOfWork.Menus.AddItemAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return ToMenuItemDto(item);
    }

    // 'private' — only callable within this class (helper/mapper method)
    // 'static' — belongs to the type itself, not to any instance; no 'this' reference needed
    // '=>' — expression-bodied member: concise single-expression method body
    // 'new()' — target-typed new expression; compiler infers the type from the return type
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
        // '.ToString()' — converts the enum value to its string name for serialisation
        Status = restaurant.Status.ToString(),
        LogoUrl = restaurant.LogoUrl
    };

    // 'private static' — pure mapping helper; no side effects, no instance state required
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
