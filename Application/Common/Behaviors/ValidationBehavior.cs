using FluentValidation;
using MediatR;
using Application.Common.Models;

namespace Application.Common.Behaviors;
// Config for returning all vcalidations too
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Any())
        {
            var validationErrors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
            
            if (typeof(TResponse) == typeof(ResponseObjectJsonDto))
            {
                var response = new ResponseObjectJsonDto
                {
                    Message = "Validation failed",
                    Code = 400,
                    Response = null,
                    ValidationErrors = validationErrors
                };
                
                return (TResponse)(object)response;
            }
        }

        return await next();
    }
}
