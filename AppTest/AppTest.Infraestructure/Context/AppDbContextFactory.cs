using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppTest.Infraestructure.Context;

/// <summary>
/// Fábrica usada apenas em tempo de design (comandos "dotnet ef") para
/// criar o <see cref="AppDbContext"/> sem depender do projeto de inicialização.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("APPTEST_CONNECTION")
            ?? "Server=localhost;Port=5437;Database=db_apptest;User Id=postgres;Password=123456789;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
