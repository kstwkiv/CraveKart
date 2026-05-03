using FoodFleet.Shared.Events.Restaurants;
using FoodFleet.Shared.Messaging.Interfaces;
using MediatR;
using Restaurant.API.Application.Commands;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Domain.Enums;

namespace Restaurant.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="CreateRestaurantCommand"/> requests.
/// Creates a new restaurant with Pending status awaiting admin approval.
/// </summary>
public class CreateRestaurantHandler : IRequestHandler<CreateRestaurantCommand, RestaurantDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateRestaurantHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    /// <param name="eventPublisher">The event publisher for raising domain events.</param>
    public CreateRestaurantHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the create restaurant request by persisting the new restaurant and returning its DTO.
    /// </summary>
    /// <param name="request">The command containing restaurant details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="RestaurantDto"/> representing the newly created restaurant.</returns>
    public async Task<RestaurantDto> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = new Domain.Entities.Restaurant
        {
            OwnerId = request.OwnerId,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Lat = request.Lat,
            Lng = request.Lng,
            CuisineTypes = request.CuisineTypes,
            OperatingHours = request.OperatingHours,
            MinimumOrderAmount = request.MinimumOrderAmount,
            EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes,
            Status = RestaurantStatus.Pending
        };

        await _unitOfWork.Restaurants.AddAsync(restaurant);
        await _unitOfWork.SaveChangesAsync();

        return new RestaurantDto
        {
            Id = restaurant.Id,
            OwnerId = restaurant.OwnerId,
            Name = restaurant.Name,
            Description = restaurant.Description,
            Address = restaurant.Address,
            CuisineTypes = restaurant.CuisineTypes,
            Status = restaurant.Status.ToString(),
            IsOpen = restaurant.IsOpen,
            MinimumOrderAmount = restaurant.MinimumOrderAmount,
            EstimatedDeliveryMinutes = restaurant.EstimatedDeliveryMinutes
        };
    }
}