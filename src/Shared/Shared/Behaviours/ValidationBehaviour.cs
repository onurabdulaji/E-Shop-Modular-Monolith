using FluentValidation;
using MediatR;
using Shared.Contracts.CQRS;

namespace Shared.Behaviours;
public class ValidationBehaviour<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>

{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults =
            await Task.WhenAll(validators.Select(p => p.ValidateAsync(context, cancellationToken)));

        var failues =
            validationResults
            .Where(p => p.Errors.Any())
            .SelectMany(p => p.Errors)
            .ToList();

        if (failues.Any()) throw new ValidationException(failues);

        return await next();
    }
}
