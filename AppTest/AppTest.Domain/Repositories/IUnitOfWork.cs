namespace AppTest.Domain.Repositories;

/// <summary>
/// Unit of Work: coordena os repositórios e persiste as alterações
/// dentro de uma única transação/contexto.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }

    IRefreshTokenRepository RefreshTokens { get; }

    ICategoriaRepository Categorias { get; }

    IProdutoRepository Produtos { get; }

    /// <summary>Persiste todas as alterações pendentes no banco.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
