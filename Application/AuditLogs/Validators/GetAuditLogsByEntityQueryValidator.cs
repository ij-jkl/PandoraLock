using Application.AuditLogs.Queries;
using FluentValidation;

namespace Application.AuditLogs.Validators;

public class GetAuditLogsByEntityQueryValidator : AbstractValidator<GetAuditLogsByEntityQuery>
{
    public GetAuditLogsByEntityQueryValidator()
    {
        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required")
            .MaximumLength(100).WithMessage("Entity name must not exceed 100 characters");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required")
            .MaximumLength(100).WithMessage("Entity ID must not exceed 100 characters");
    }
}
