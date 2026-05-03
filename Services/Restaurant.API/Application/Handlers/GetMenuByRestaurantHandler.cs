using MediatR;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Queries;

namespace Restaurant.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="GetMenuByRestaurantQuery"/> requests.
/// Retrieves all menu items for a restaurant, flattened across all categories.
/// </summary>
public class GetMenuByRestaurantHandler : IRequestHandler<GetMenuByRestaurantQuery, List<MenuItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="GetMenuByRestaurantHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public GetMenuByRestaurantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query by fetching all menu categories with items and flattening them into a list.
    /// </summary>
    /// <param name="request">The query containing the restaurant ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A flat list of <see cref="MenuItemDto"/> records across all categories.</returns>
    public async Task<List<MenuItemDto>> Handle(GetMenuByRestaurantQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Menus.GetCategoriesWithItemsAsync(request.RestaurantId);

        return categories
            .SelectMany(c => c.MenuItems)
            .Select(i => new MenuItemDto
            {
                Id = i.Id,
                CategoryId = i.CategoryId,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                ImageUrl = i.ImageUrl,
                IsAvailable = i.IsAvailable,
                DietaryTags = i.DietaryTags
            }).ToList();
    }
}