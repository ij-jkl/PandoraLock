using Application.AuditLogs.Queries;
using FluentValidation;

namespace Application.AuditLogs.Validators;

public class GetAuditLogsByDateRangeQueryValidator : AbstractValidator<GetAuditLogsByDateRangeQuery>
{
    public GetAuditLogsByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");
    }
}
