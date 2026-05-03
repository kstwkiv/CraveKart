using MediatR;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Queries;

namespace Restaurant.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="GetRestaurantByIdQuery"/> requests.
/// Retrieves a single restaurant by its unique identifier.
/// </summary>
public class GetRestaurantByIdHandler : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="GetRestaurantByIdHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public GetRestaurantByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query by fetching the restaurant and mapping it to a DTO.
    /// </summary>
    /// <param name="request">The query containing the restaurant ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="RestaurantDto"/> if found; otherwise <c>null</c>.</returns>
    public async Task<RestaurantDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _unitOfWork.Restaurants.GetByIdAsync(request.RestaurantId);
        if (r == null) return null;

        return new RestaurantDto
        {
            Id = r.Id,
            OwnerId = r.OwnerId,
            Name = r.Name,
            Description = r.Description,
            Address = r.Address,
            CuisineTypes = r.CuisineTypes,
            AverageRating = r.AverageRating,
            TotalReviews = r.TotalReviews,
            IsOpen = r.IsOpen,
            EstimatedDeliveryMinutes = r.EstimatedDeliveryMinutes,
            MinimumOrderAmount = r.MinimumOrderAmount,
            Status = r.Status.ToString(),
            LogoUrl = r.LogoUrl
        };
    }
}