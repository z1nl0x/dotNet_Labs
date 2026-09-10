using AppTest.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace AppTest.Application.Common.Behaviors;

/// <summary>
/// Behavior do pipeline do MediatR que executa todos os <see cref="IValidator{T}"/>
/// registrados para o request antes de chegar ao handler. Falhas viram uma
/// <see cref="BadRequestException"/> (400), tratada de forma centralizada pelo
/// middleware da API — os handlers ficam livres de lógica de validação.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = (await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToList();

            if (failures.Count != 0)
                throw new BadRequestException(string.Join(" ", failures.Select(f => f.ErrorMessage)));
        }

        return await next();
    }
}
