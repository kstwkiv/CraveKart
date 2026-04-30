using Identity.API.Application.DTOs;

namespace Identity.API.Application.Commands;

/// <summary>
/// Command to authenticate an existing user and issue a JWT access token.
/// </summary>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Password">The user's plain-text password for verification.</param>
/// A <record>is a special type in C# used to represent immutable data models (data that shouldn’t change after creation).
public record LoginCommand(
    string Email,
    string Password);
