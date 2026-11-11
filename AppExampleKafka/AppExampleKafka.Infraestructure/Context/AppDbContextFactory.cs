using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppExampleKafka.Infraestructure.Context;

/// <summary>
/// Fábrica usada apenas em tempo de design (comandos "dotnet ef") para
/// criar o <see cref="AppDbContext"/> sem depender do projeto de inicialização.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("APPEXAMPLEKAFKA_CONNECTION")
            ?? "Host=localhost;Port=15437;Database=apptest_bancario;Username=apptest;Password=somente-dev-troque-esta-senha";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
