using MediatR;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Queries;

namespace Restaurant.API.Application.Handlers;

public class GetMenuByRestaurantHandler : IRequestHandler<GetMenuByRestaurantQuery, List<MenuItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMenuByRestaurantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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