using Application.Common.Interfaces;
using Application.Files.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Files.Validators;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public UploadFileCommandValidator(IFileTypeValidator fileTypeValidator)
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .When(x => x.File != null)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .When(x => x.File != null)
            .WithMessage("File size exceeds the maximum allowed size of 10MB");

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .When(x => x.File != null)
            .WithMessage("File name is required");

        RuleFor(x => x.File)
            .Must(file =>
            {
                if (file == null) return true;
                using var stream = file.OpenReadStream();
                return fileTypeValidator.IsValidFileType(stream, out var detectedType) &&
                       fileTypeValidator.IsAllowedFileType(detectedType);
            })
            .When(x => x.File != null)
            .WithMessage("Only PDF, JPG, and PNG files are allowed. The file content must match its type.");
    }
}

