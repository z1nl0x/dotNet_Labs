using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AppExampleKafka.Application.Features.Auth.Register;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AppExampleKafka.Tests;

public class SwaggerTests
{
    [Fact]
    public async Task Swagger_e_assemblies_identificam_AppExampleKafka()
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b.UseEnvironment("Development"));
        using var client = factory.CreateClient();
        var doc = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        Assert.Equal("AppExampleKafka API", doc.GetProperty("info").GetProperty("title").GetString());
        var assemblies = new[]
        {
            typeof(AppExampleKafka.API.Controllers.AuthController).Assembly,
            typeof(RegisterCommand).Assembly,
            typeof(AppExampleKafka.Domain.Entities.Bancario.Transacao).Assembly,
            typeof(AppExampleKafka.Infraestructure.Context.AppDbContext).Assembly,
            typeof(SwaggerTests).Assembly
        };
        Assert.All(assemblies, assembly => Assert.StartsWith("AppExampleKafka.", assembly.GetName().Name!));
        Assert.Contains("AppExampleKafka API — Swagger", await client.GetStringAsync("/swagger/index.html"));
    }

    [Fact]
    public async Task Swagger_documenta_contratos_exemplos_e_autenticacao_por_operacao()
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b.UseEnvironment("Development"));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var doc = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var paths = doc.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        foreach (var verb in path.Value.EnumerateObject())
        {
            var op = verb.Value;
            Assert.False(string.IsNullOrWhiteSpace(op.GetProperty("summary").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(op.GetProperty("description").GetString()));
            var anonymous = path.Name is "/api/Auth/register" or "/api/Auth/login" or "/api/Auth/refresh";
            Assert.Equal(anonymous ? 0 : 1, op.GetProperty("security").GetArrayLength());
        }
        var register = paths.GetProperty("/api/Auth/register").GetProperty("post");
        var examples = register.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("examples");
        foreach (var name in new[] { "PF", "PJ" })
        {
            var payload = examples.GetProperty(name).GetProperty("value");
            var command = payload.Deserialize<RegisterCommand>(new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            });
            Assert.True((await new RegisterCommandValidator().ValidateAsync(command!)).IsValid);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/register", payload)).StatusCode);
        }
        Assert.False(paths.GetProperty("/api/Auth/me").GetProperty("get").GetProperty("responses").TryGetProperty("404", out _));
        var contas = paths.GetProperty("/api/contas");
        Assert.False(contas.GetProperty("post").TryGetProperty("requestBody", out _));
        Assert.Contains("ContaBancariaDto", contas.GetProperty("post").GetProperty("responses").GetProperty("201").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()!);
        Assert.Contains("PagedResult", contas.GetProperty("get").GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()!);
        Assert.Equal(100, contas.GetProperty("get").GetProperty("parameters")[1].GetProperty("schema").GetProperty("maximum").GetInt32());
        var transfer = paths.GetProperty("/api/transacoes").GetProperty("post");
        Assert.True(transfer.GetProperty("responses").TryGetProperty("409", out _));
        Assert.True(transfer.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").TryGetProperty("examples", out _));
        Assert.True(paths.GetProperty("/api/Auth/admin-only").GetProperty("get").GetProperty("responses").TryGetProperty("403", out _));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/index.html")).StatusCode);
    }
}
