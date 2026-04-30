using FluentValidation;
using Identity.API.Application.Commands;

namespace Identity.API.Application.Validators;

/// <summary>
/// FluentValidation validator for <see cref="RegisterUserCommand"/>.
/// Enforces name length, email format, password complexity, and mobile number requirements.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Initializes a new instance of <see cref="RegisterRequestValidator"/> with validation rules.</summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(20);
    }
}