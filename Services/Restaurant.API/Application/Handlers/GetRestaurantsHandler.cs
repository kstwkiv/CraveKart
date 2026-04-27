using MediatR;
using Restaurant.API.Application.DTOs;
using Restaurant.API.Application.Interfaces;
using Restaurant.API.Application.Queries;

namespace Restaurant.API.Application.Handlers;

public class GetRestaurantsHandler : IRequestHandler<GetRestaurantsQuery, List<RestaurantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRestaurantsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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