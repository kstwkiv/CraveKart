using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Application.DTOs;
using Order.API.Application.Interfaces;
using Order.API.Domain.Enums;

namespace Order.API.Controllers;

[ApiController]
[Route("api/v1/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminOrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
        {
            var filtered = await _unitOfWork.Orders.GetByStatusAsync(parsed);
            return Ok(filtered.Select(ToDto));
        }

        var all = await _unitOfWork.Orders.GetAllAsync();
        return Ok(all.Select(ToDto));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var all = (await _unitOfWork.Orders.GetAllAsync()).ToList();

        return Ok(new
        {
            Total = all.Count,
            Placed = all.Count(o => o.Status == OrderStatus.Placed),
            Confirmed = all.Count(o => o.Status == OrderStatus.Confirmed),
            Preparing = all.Count(o => o.Status == OrderStatus.Preparing),
            Ready = all.Count(o => o.Status == OrderStatus.Ready),
            Delivered = all.Count(o => o.Status == OrderStatus.Delivered),
            Cancelled = all.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = all
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.TotalAmount)
        });
    }

    // GET /api/v1/admin/orders/reports/csv
    [HttpGet("reports/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var all = (await _unitOfWork.Orders.GetAllAsync()).AsEnumerable();

        if (Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            all = all.Where(o => o.Status == parsed);

        if (from.HasValue) all = all.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue)   all = all.Where(o => o.CreatedAt <= to.Value);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("OrderId,CustomerId,RestaurantId,Status,TotalAmount,PaymentMethod,CreatedAt");

        foreach (var o in all)
            sb.AppendLine($"{o.Id},{o.CustomerId},{o.RestaurantId},{o.Status},{o.TotalAmount},{o.PaymentMethod},{o.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"orders_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static OrderDto ToDto(Domain.Entities.Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        RestaurantId = o.RestaurantId,
        DeliveryAddress = o.DeliveryAddress,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        PaymentMethod = o.PaymentMethod.ToString(),
        CreatedAt = o.CreatedAt,
        Items = o.OrderItems.Select(i => new OrderItemDto
        {
            MenuItemId = i.MenuItemId,
            MenuItemName = i.MenuItemName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Customizations = i.Customizations
        }).ToList()
    };
}
