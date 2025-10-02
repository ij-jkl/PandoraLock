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
            var validationErrors = failures.Select(f => f.ErrorMessage).ToList();
            
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ResponseObjectJsonDto<>))
            {
                var responseType = typeof(TResponse).GetGenericArguments()[0];
                var responseObjectType = typeof(ResponseObjectJsonDto<>).MakeGenericType(responseType);
                
                var response = Activator.CreateInstance(responseObjectType);
                responseObjectType.GetProperty("Message")?.SetValue(response, "Validation failed");
                responseObjectType.GetProperty("Code")?.SetValue(response, 400);
                responseObjectType.GetProperty("Response")?.SetValue(response, null);
                responseObjectType.GetProperty("ValidationErrors")?.SetValue(response, validationErrors);
                
                return (TResponse)response;
            }
        }

        return await next();
    }
}
