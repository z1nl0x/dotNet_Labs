using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AppExampleKafka.Application.Common.Exceptions;

namespace AppExampleKafka.Infraestructure.UnitOfWork;

/// <summary>
/// Implementação do Unit of Work sobre o <see cref="AppDbContext"/>.
/// Expõe os repositórios e centraliza o commit das alterações.
/// </summary>
public class UnitOfWork(
    AppDbContext context,
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IContaBancariaRepository contas,
    ITransacaoRepository transacoes) : IUnitOfWork
{
    public ITransacaoRepository Transacoes { get; } = transacoes;
    public async Task<T> ExecutarTransacaoAsync<T>(Func<Task<T>> acao, CancellationToken ct)
    {
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var result = await acao();
            await tx.CommitAsync(ct);
            return result;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(CancellationToken.None);
            context.ChangeTracker.Clear();
            if (EhConflito(ex)) throw new ConflictException("Conflito concorrente. Repita a requisição com a mesma chave de idempotência.");
            throw;
        }
    }
    public IContaBancariaRepository Contas { get; } = contas;
    public IUserRepository Users { get; } = users;

    public IRefreshTokenRepository RefreshTokens { get; } = refreshTokens;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { return await context.SaveChangesAsync(cancellationToken); }
        catch (Exception ex) when (EhConflito(ex))
        {
            throw new ConflictException("Conflito de unicidade ou concorrência. Repita com a mesma chave quando aplicável.");
        }
    }

    private static bool EhConflito(Exception ex)
    {
        for (Exception? atual = ex; atual is not null; atual = atual.InnerException)
            if (atual is DbUpdateConcurrencyException || atual is PostgresException { SqlState: "40001" or "40P01" or "23505" }) return true;
        return false;
    }
}
