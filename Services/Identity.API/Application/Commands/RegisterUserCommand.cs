using Identity.API.Application.DTOs;

namespace Identity.API.Application.Commands;

/// <summary>
/// Command to register a new user account in the system.
/// </summary>
/// <param name="FullName">The full name of the user.</param>
/// <param name="Email">The email address for the new account.</param>
/// <param name="Password">The plain-text password to be hashed and stored.</param>
/// <param name="MobileNumber">The user's mobile phone number.</param>
/// <param name="Role">The role to assign (e.g., "Customer", "RestaurantOwner", "DeliveryAgent").</param>
/// A <record> is a special type in C# used to represent immutable data models (data that shouldn’t change after creation).
public record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string MobileNumber,
    string Role);
