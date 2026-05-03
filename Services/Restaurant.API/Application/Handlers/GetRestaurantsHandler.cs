using MediatR;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Queries;

namespace Restaurant.API.Application.Handlers;

/// <summary>
/// MediatR handler that processes <see cref="GetRestaurantsQuery"/> requests.
/// Retrieves all active restaurants, optionally filtered by a search term.
/// </summary>
public class GetRestaurantsHandler : IRequestHandler<GetRestaurantsQuery, List<RestaurantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="GetRestaurantsHandler"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public GetRestaurantsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Handles the query by fetching active restaurants and mapping them to DTOs.
    /// </summary>
    /// <param name="request">The query containing an optional search term.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of <see cref="RestaurantDto"/> records.</returns>
    public async Task<List<RestaurantDto>> Handle(GetRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var restaurants = string.IsNullOrEmpty(request.SearchTerm)
            ? await _unitOfWork.Restaurants.GetAllActiveAsync()
            : await _unitOfWork.Restaurants.SearchAsync(request.SearchTerm);

        return restaurants.Select(r => new RestaurantDto
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
        }).ToList();
    }
}