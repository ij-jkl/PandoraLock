using Application.Files.Commands;
using FluentValidation;

namespace Application.Files.Validators;

public class ShareFileCommandValidator : AbstractValidator<ShareFileCommand>
{
    public ShareFileCommandValidator()
    {
        RuleFor(x => x.FileId)
            .GreaterThan(0)
            .WithMessage("File ID must be greater than 0");

        RuleFor(x => x.SharedWithEmail)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0)
            .WithMessage("Owner ID must be greater than 0");
    }
}
