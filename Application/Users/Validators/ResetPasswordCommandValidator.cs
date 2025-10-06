using Application.Users.Commands;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Users.Validators;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Must(ContainNumber).WithMessage("Password must contain at least one number")
            .Must(ContainUppercase).WithMessage("Password must contain at least one uppercase letter")
            .Must(ContainLowercase).WithMessage("Password must contain at least one lowercase letter")
            .Must(ContainSymbol).WithMessage("Password must contain at least one symbol");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword).WithMessage("Confirm password must match the new password field");
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

