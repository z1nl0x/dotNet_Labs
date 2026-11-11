using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Application.Features.Auth.Register;
using AppExampleKafka.Application.DTOs.Contas;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppExampleKafka.Tests;

public class ApiFactory : WebApplicationFactory<AppExampleKafka.API.Controllers.AuthController>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Kafka:Enabled", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(connection));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
}

public class ApiTests
{
    [Fact]
    public async Task Endpoints_bancarios_exigem_JWT_e_criacao_usa_titular_do_token()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/contas" )).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/transacoes" )).StatusCode);
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterCommand("Ana", "ana@example.com", "SenhaForte123!", OnboardingTests.PF));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var me = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/auth/me");
        Assert.Equal("52998224725", me.GetProperty("cadastro").GetProperty("documento").GetString());
        Assert.False(me.TryGetProperty("passwordHash", out _));
        var create = await client.PostAsJsonAsync("/api/contas", new { usuarioId = Guid.NewGuid(), saldo = 100000 });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var conta = (await create.Content.ReadFromJsonAsync<ContaBancariaDto>())!;
        Assert.Equal(0m, conta.Saldo);
        Assert.Equal("0001", conta.Agencia);
        Assert.Equal("Corrente", conta.TipoConta);
        Assert.Matches("^[0-9]{12}$", conta.Numero);
        Assert.Matches("^[0-9]$", conta.DigitoVerificador);
        Assert.Equal(conta, await client.GetFromJsonAsync<ContaBancariaDto>(create.Headers.Location));
        var empty = await client.PostAsync("/api/contas", null);
        Assert.Equal(HttpStatusCode.Created, empty.StatusCode);
        var outra = (await empty.Content.ReadFromJsonAsync<ContaBancariaDto>())!;
        Assert.NotEqual(conta.Numero, outra.Numero);
        Assert.Equal(0m, outra.Saldo);
        Assert.Equal("Corrente", outra.TipoConta);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/produtos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/categorias")).StatusCode);
    }
}
