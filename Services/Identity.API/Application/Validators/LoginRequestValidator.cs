using FluentValidation;
using Identity.API.Application.Commands;

namespace Identity.API.Application.Validators;

/// <summary>
/// FluentValidation validator for <see cref="LoginCommand"/>.
/// Ensures the email is valid and the password is not empty.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes a new instance of <see cref="LoginRequestValidator"/> with validation rules.</summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}