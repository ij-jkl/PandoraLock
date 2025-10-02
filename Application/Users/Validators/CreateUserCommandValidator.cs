using Application.Users.Commands;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Users.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserData.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email format");

        RuleFor(x => x.UserData.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long");

        RuleFor(x => x.UserData.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Must(ContainNumber).WithMessage("Password must contain at least one number")
            .Must(ContainUppercase).WithMessage("Password must contain at least one uppercase letter")
            .Must(ContainLowercase).WithMessage("Password must contain at least one lowercase letter")
            .Must(ContainSymbol).WithMessage("Password must contain at least one symbol");

        RuleFor(x => x.UserData.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required")
            .Equal(x => x.UserData.Password).WithMessage("Confirm Password must match the password field");
    }

    private static bool ContainNumber(string password)
        => Regex.IsMatch(password, @"\d");

    private static bool ContainUppercase(string password)
        => Regex.IsMatch(password, @"[A-Z]");

    private static bool ContainLowercase(string password)
        => Regex.IsMatch(password, @"[a-z]");

    private static bool ContainSymbol(string password)
        => Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");
}
