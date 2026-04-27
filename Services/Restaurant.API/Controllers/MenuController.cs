using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly IUnitOfWork _unitOfWork;

    public MenuController(IRestaurantService restaurantService, IUnitOfWork unitOfWork)
    {
        _restaurantService = restaurantService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMenu(Guid restaurantId)
    {
        var categories = await _unitOfWork.Menus.GetCategoriesWithItemsAsync(restaurantId);
        var result = categories.Select(c => new MenuCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Items = c.MenuItems.Select(i => new MenuItemDto
            {
                Id = i.Id,
                CategoryId = i.CategoryId,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                ImageUrl = i.ImageUrl,
                IsAvailable = i.IsAvailable,
                DietaryTags = i.DietaryTags
            }).ToList()
        });
        return Ok(result);
    }

    [HttpPost("restaurant/{restaurantId}/categories")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> CreateCategory(Guid restaurantId, [FromBody] CreateMenuCategoryRequest request)
    {
        var category = new MenuCategory
        {
            RestaurantId = restaurantId,
            Name = request.Name,
            SortOrder = request.SortOrder
        };
        await _unitOfWork.Menus.AddCategoryAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new MenuCategoryDto { Id = category.Id, Name = category.Name, Items = new() });
    }

    [HttpPost("restaurant/{restaurantId}/items")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> CreateItem(Guid restaurantId, [FromBody] CreateMenuItemRequest request)
    {
        var command = new CreateMenuItemCommand(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.DietaryTags,
            request.ImageUrl);

        var result = await _restaurantService.CreateMenuItemAsync(command);
        return Ok(result);
    }

    [HttpPatch("items/{itemId}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateMenuItemRequest request)
    {
        var item = await _unitOfWork.Menus.GetItemByIdAsync(itemId);
        if (item == null) return NotFound();

        item.Name = request.Name ?? item.Name;
        item.Description = request.Description ?? item.Description;
        item.Price = request.Price ?? item.Price;
        item.IsAvailable = request.IsAvailable ?? item.IsAvailable;
        item.ImageUrl = request.ImageUrl ?? item.ImageUrl;
        item.DietaryTags = request.DietaryTags ?? item.DietaryTags;

        _unitOfWork.Menus.UpdateItem(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new MenuItemDto
        {
            Id = item.Id, CategoryId = item.CategoryId, Name = item.Name,
            Description = item.Description, Price = item.Price,
            ImageUrl = item.ImageUrl, IsAvailable = item.IsAvailable, DietaryTags = item.DietaryTags
        });
    }

    [HttpDelete("items/{itemId}")]
    [Authorize(Roles = "RestaurantOwner,Admin")]
    public async Task<IActionResult> DeleteItem(Guid itemId)
    {
        var item = await _unitOfWork.Menus.GetItemByIdAsync(itemId);
        if (item == null) return NotFound();
        _unitOfWork.Menus.DeleteItem(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
