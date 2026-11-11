using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using AppExampleKafka.Infraestructure.Repositories;
using AppExampleKafka.Infraestructure.Repositories.Users;
using AppExampleKafka.Infraestructure.Security;
using AppExampleKafka.Infraestructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppExampleKafka.Infraestructure;

/// <summary>Registro das dependências da camada de Infraestrutura.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddOptions<KafkaOptions>().Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.BootstrapServers) && !string.IsNullOrWhiteSpace(o.Topic), "Kafka: servidor e tópico obrigatórios.")
            .Validate(o => o.PollIntervalSeconds is >= 1 and <= 300 && o.MessageTimeoutMs is >= 1000 and <= 120000, "Kafka: intervalos inválidos.")
            .ValidateOnStart();
        services.AddSingleton<IEventoPublisher, KafkaEventoPublisher>();
        services.AddScoped<OutboxDispatcher>();
        services.AddHostedService<OutboxWorker>();

        services.AddScoped<IUserRepository, UsuarioRepository>();
        services.AddScoped<IContaBancariaRepository, ContaBancariaRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
