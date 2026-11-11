using System.Reflection;
using AppExampleKafka.Domain.Entities;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Context;

/// <summary>Contexto do Entity Framework Core (PostgreSQL).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ContaBancaria> Contas => Set<ContaBancaria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<OutboxMensagem> Outbox => Set<OutboxMensagem>();
    public DbSet<Usuario> Users => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
