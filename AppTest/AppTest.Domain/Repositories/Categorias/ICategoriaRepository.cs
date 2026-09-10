using AppTest.Domain.Entities.Categorias;

namespace AppTest.Domain.Repositories;

/// <summary>Contrato de acesso a dados para a entidade <see cref="Category"/>.</summary>
public interface ICategoriaRepository
{
    Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna uma página de categorias, opcionalmente filtradas por nome (contém, case-insensitive),
    /// junto com o total de registros que atendem ao filtro.
    /// </summary>
    Task<(IReadOnlyList<Categoria> Items, int TotalCount)> ListAsync(
        string? nome,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default);

    void Remove(Categoria categoria);
}
