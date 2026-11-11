using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Catalogo_foi_removido_do_modelo_e_dos_endpoints()
    {
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=:memory:").Options);
        Assert.DoesNotContain(db.Model.GetEntityTypes(), x => x.ClrType.Name is "Produto" or "Categoria");
        Assert.DoesNotContain(typeof(AppExampleKafka.API.Controllers.AuthController).Assembly.GetTypes(), x => x.Name is "ProdutosController" or "CategoriasController");
    }
}
