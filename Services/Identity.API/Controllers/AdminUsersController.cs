using Identity.API.Application.Interfaces;
using Identity.API.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

/// <summary>
/// API controller for admin-level user management operations.
/// Provides endpoints for listing, activating, and deactivating user accounts.
/// Accessible by Admin role only.
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminUsersController"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    public AdminUsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Retrieves all user accounts, optionally filtered by role.
    /// </summary>
    /// <param name="role">Optional role filter (e.g., "Customer", "Admin").</param>
    /// <returns>A list of user DTOs.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? role)
    {
        var users = await _unitOfWork.Users.GetAllAsync();

        if (!string.IsNullOrEmpty(role))
            users = users.Where(u => u.Role.ToString().Equals(role, StringComparison.OrdinalIgnoreCase));

        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role.ToString(),
            MobileNumber = u.MobileNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }));
    }

    /// <summary>
    /// Deactivates a user account by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user to deactivate.</param>
    /// <returns>200 OK with a confirmation message, or 404 if not found.</returns>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "User deactivated." });
    }

    /// <summary>
    /// Activates a previously deactivated user account by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user to activate.</param>
    /// <returns>200 OK with a confirmation message, or 404 if not found.</returns>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        user.UpdatedAt = IstClock.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "User activated." });
    }
}
