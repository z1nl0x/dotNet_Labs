using System.Reflection;
using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Categorias;
using AppTest.Domain.Entities.Produtos;
using AppTest.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace AppTest.Infraestructure.Context;

/// <summary>Contexto do Entity Framework Core (PostgreSQL).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Users => Set<Usuario>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
