using AppTest.Domain.Entities.Common;

namespace AppTest.Domain.Entities.Categorias;

/// <summary>
/// Representa uma categoria à qual os produtos pertencem.
/// </summary>
public class Categoria : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    // Construtor sem parâmetros exigido pelo EF Core.
    private Categoria() { }

    public Categoria(string nome, string? descricao = null)
    {
        Nome = nome;
        Descricao = descricao;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Update(string nome, string? descricao)
    {
        Nome = nome;
        Descricao = descricao;
        AtualizadoEm = DateTime.UtcNow;
    }
}
