using AppExampleKafka.Application;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.DTOs.Contas;
using AppExampleKafka.Application.Features.Contas.Create;
using AppExampleKafka.Application.Features.Contas.GetById;
using AppExampleKafka.Application.Features.Contas.GetList;
using AppExampleKafka.Domain.Entities.Usuarios;
using AppExampleKafka.Infraestructure;
using AppExampleKafka.Infraestructure.Context;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppExampleKafka.Tests;

public sealed class BankingFixture : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public ServiceProvider Provider { get; }
    public IServiceScope Scope { get; }
    public AppDbContext Db => Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    public ISender Sender => Scope.ServiceProvider.GetRequiredService<ISender>();
    public TestUser User { get; } = new();
    public BankingFixture(Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor? interceptor = null)
    {
        connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:DefaultConnection"] = "Host=unused;Database=unused" }).Build());
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlite(connection);
            if (interceptor is not null) o.AddInterceptors(interceptor);
        });
        services.AddScoped<IUsuarioAtual>(_ => User);
        services.Configure<AppExampleKafka.Infraestructure.Security.JwtSettings>(o =>
        {
            o.Issuer = "Tests";
            o.Audience = "Tests";
            o.SecretKey = "test-only-key-with-at-least-32-characters-123456789";
        });
        Provider = services.BuildServiceProvider();
        Scope = Provider.CreateScope();
        Db.Database.EnsureCreated();
        var usuario = new Usuario("Ana", "ana@example.com", "hash");
        User.Id = usuario.Id;
        Db.Users.Add(usuario);
        Db.SaveChanges();
    }
    public async ValueTask DisposeAsync() { Scope.Dispose(); await Provider.DisposeAsync(); await connection.DisposeAsync(); }
}

public sealed class TestUser : IUsuarioAtual
{
    public Guid Id { get; set; }
}

public class AccountTests
{
    [Fact]
    public async Task Banco_rejeita_numero_duplicado_por_indice_unico()
    {
        await using var f = new BankingFixture();
        var primeira = await f.Sender.Send(new CriarContaCommand());
        var segunda = new AppExampleKafka.Domain.Entities.Bancario.ContaBancaria(f.User.Id);
        f.Db.Entry(segunda).Property(x => x.Numero).CurrentValue = primeira.Numero;
        f.Db.Contas.Add(segunda);
        await Assert.ThrowsAsync<DbUpdateException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Numeros_gerados_sao_distintos_numericos_e_tem_modulo_11_valido()
    {
        await using var f = new BankingFixture();
        var numeros = new HashSet<string>();
        for (var i = 0; i < 100; i++)
        {
            var conta = await f.Sender.Send(new CriarContaCommand());
            Assert.Matches("^[0-9]{12}$", conta.Numero);
            Assert.True(numeros.Add(conta.Numero));
            var soma = 0;
            for (var pos = 11; pos >= 0; pos--) soma += (conta.Numero[pos] - '0') * (2 + (11 - pos) % 8);
            var esperado = soma % 11 <= 1 ? 0 : 11 - soma % 11;
            Assert.Equal(esperado.ToString(), conta.DigitoVerificador);
        }
    }

    [Fact]
    public async Task Criacao_gera_identificacao_bancaria_e_expoe_data_de_abertura_persistidas()
    {
        await using var f = new BankingFixture();
        var inicio = DateTime.UtcNow;
        var conta = await f.Sender.Send(new CriarContaCommand());
        var json = System.Text.Json.JsonSerializer.SerializeToElement(conta);
        Assert.Equal("0001", json.GetProperty("Agencia").GetString());
        Assert.Matches("^[0-9]{12}$", json.GetProperty("Numero").GetString()!);
        Assert.Matches("^[0-9]$", json.GetProperty("DigitoVerificador").GetString()!);
        Assert.Equal("Corrente", json.GetProperty("TipoConta").GetString());
        Assert.InRange(json.GetProperty("CriadoEm").GetDateTime(), inicio, DateTime.UtcNow);
        f.Db.ChangeTracker.Clear();
        var stored = await f.Db.Contas.SingleAsync();
        Assert.Equal(conta, new ContaBancariaDto(stored.Id, stored.Saldo, stored.Moeda, stored.Ativa,
            stored.Agencia, stored.Numero, stored.DigitoVerificador, stored.TipoConta, stored.CriadoEm));
        Assert.Equal(conta, await f.Sender.Send(new ObterContaQuery(conta.Id)));
        Assert.Equal(conta, Assert.Single((await f.Sender.Send(new ListarContasQuery())).Items));
    }

    [Fact]
    public async Task Consultas_nao_expoem_contas_de_outro_titular()
    {
        await using var f = new BankingFixture();
        var conta = await f.Sender.Send(new CriarContaCommand());
        f.User.Id = Guid.NewGuid();
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.NotFoundException>(() => f.Sender.Send(new ObterContaQuery(conta.Id)));
        var lista = await f.Sender.Send(new ListarContasQuery());
        Assert.Empty(lista.Items);
    }

    [Fact]
    public async Task Criacao_via_CQRS_persiste_conta_do_usuario_atual_sem_aporte()
    {
        await using var f = new BankingFixture();
        var conta = await f.Sender.Send(new CriarContaCommand());
        f.Db.ChangeTracker.Clear();
        var stored = await f.Db.Contas.SingleAsync();
        Assert.Equal(conta.Id, stored.Id);
        Assert.Equal(f.User.Id, stored.UsuarioId);
        Assert.Equal(0m, stored.Saldo);
    }
}
