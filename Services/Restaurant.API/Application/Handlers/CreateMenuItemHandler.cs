using MediatR;
using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="CreateMenuItemCommand"/> requests.
/// Creates a new menu item within the specified category.
/// </summary>
public class CreateMenuItemHandler : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMenuItemHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public CreateMenuItemHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the create menu item request by persisting the item and returning its DTO.
    /// </summary>
    /// <param name="request">The command containing menu item details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="MenuItemDto"/> representing the newly created menu item.</returns>
    public async Task<MenuItemDto> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = new MenuItem
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DietaryTags = request.DietaryTags,
            IsAvailable = true
        };

        await _unitOfWork.Menus.AddItemAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return new MenuItemDto
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            IsAvailable = item.IsAvailable,
            DietaryTags = item.DietaryTags
        };
    }
}