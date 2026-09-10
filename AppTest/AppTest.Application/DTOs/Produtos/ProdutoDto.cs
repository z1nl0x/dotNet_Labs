namespace AppTest.Application.DTOs.Produtos;

/// <summary>Representação pública de um produto, com o nome da categoria resolvido.</summary>
public record ProdutoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    Guid CategoriaId,
    string? NomeCategoria,
    DateTime CriadoEm,
    DateTime AtualizadoEm);
