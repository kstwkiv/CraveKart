using MediatR;
using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Application.Handlers;

public class CreateMenuItemHandler : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMenuItemHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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