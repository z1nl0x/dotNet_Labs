namespace AppExampleKafka.Domain.Repositories;

/// <summary>
/// Unit of Work: coordena os repositórios e persiste as alterações
/// dentro de uma única transação/contexto.
/// </summary>
public interface IUnitOfWork
{
    IContaBancariaRepository Contas { get; }
    ITransacaoRepository Transacoes { get; }
    Task<T> ExecutarTransacaoAsync<T>(Func<Task<T>> acao, CancellationToken ct);
    IUserRepository Users { get; }

    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>Persiste todas as alterações pendentes no banco.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
