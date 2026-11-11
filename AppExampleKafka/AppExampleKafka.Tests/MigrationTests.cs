using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace AppExampleKafka.Tests;

public class MigrationTests
{
    [Fact]
    public void Inicial_e_a_unica_migration_e_cria_o_modelo_bancario_completo()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused;Database=unused").Options);
        var assembly = db.GetService<IMigrationsAssembly>();
        var item = Assert.Single(assembly.Migrations);
        Assert.EndsWith("_Inicial", item.Key);
        var migration = assembly.CreateMigration(item.Value, db.Database.ProviderName!);
        var tables = migration.UpOperations.OfType<CreateTableOperation>().ToList();
        Assert.Equal(new[] { "contas_bancarias", "outbox_mensagens", "refresh_tokens", "transacoes", "usuarios" },
            tables.Select(t => t.Name).Order().ToArray());
        var conta = Assert.Single(tables, t => t.Name == "contas_bancarias");
        foreach (var column in new[] { "Agencia", "Numero", "DigitoVerificador", "TipoConta" })
            Assert.False(Assert.Single(conta.Columns, c => c.Name == column).IsNullable);
        Assert.Contains(migration.UpOperations.OfType<CreateIndexOperation>(), i => i.Name == "IX_contas_bancarias_Numero" && i.IsUnique);
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
