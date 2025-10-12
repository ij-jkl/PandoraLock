using Application.Files.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Files.Validators;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .When(x => x.File != null)
            .WithMessage("File size exceeds the maximum allowed size of 10MB");

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .When(x => x.File != null)
            .WithMessage("File name is required");
    }
}
