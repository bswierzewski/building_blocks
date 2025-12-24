using ErrorOr;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(request, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures
            .Select(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage))
            .ToList();

        return (dynamic)errors;
    }
}