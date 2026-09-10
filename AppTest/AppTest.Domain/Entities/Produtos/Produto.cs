using AppTest.Domain.Entities.Categorias;
using AppTest.Domain.Entities.Common;

namespace AppTest.Domain.Entities.Produtos;

/// <summary>
/// Representa um produto do catálogo, sempre vinculado a uma <see cref="Category"/>.
/// </summary>
public class Produto : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public decimal Preco { get; private set; }

    public Guid CategoriaId { get; private set; }

    /// <summary>Navegação para a categoria (carregada sob demanda nas consultas).</summary>
    public Categoria? Categoria { get; private set; }

    // Construtor sem parâmetros exigido pelo EF Core.
    private Produto() { }

    public Produto(string nome, string? descricao, decimal preco, Guid categoriaId)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        CategoriaId = categoriaId;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Update(string nome, string? descricao, decimal preco, Guid categoriaId)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        CategoriaId = categoriaId;
    }
}
