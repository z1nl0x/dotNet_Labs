using System.Reflection;
using AppTest.Application.Common.Behaviors;
using AppTest.Application.Common.Interfaces;
using AppTest.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AppTest.Application;

/// <summary>Registro das dependências da camada de Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // CQRS: handlers de commands/queries descobertos por reflexão.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Validação executada no pipeline, antes de cada handler.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<ITokenIssuer, TokenIssuer>();

        return services;
    }
}
