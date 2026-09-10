using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Produtos;

namespace AppTest.Domain.Repositories;

/// <summary>Contrato de acesso a dados para a entidade <see cref="Produto"/>.</summary>
public interface IProdutoRepository
{
    Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna uma página de produtos, opcionalmente filtrados por nome (contém, case-insensitive),
    /// junto com o total de registros que atendem ao filtro.
    /// </summary>
    Task<(IReadOnlyList<Produto> Items, int TotalCount)> ListAsync(
        string? nome,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Indica se existe ao menos um produto vinculado à categoria informada.</summary>
    Task<bool> AnyByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task AddAsync(Produto produto, CancellationToken cancellationToken = default);

    void Remove(Produto produto);
}
