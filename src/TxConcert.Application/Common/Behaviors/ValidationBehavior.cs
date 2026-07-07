using TxConcert.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace TxConcert.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);

        FluentValidation.Results.ValidationResult[] results =
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));

        FluentValidation.Results.ValidationFailure[] failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length > 0)
            throw new Exceptions.ValidationException(failures);

        return await next();
    }
}
