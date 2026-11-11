using AppExampleKafka.Domain.Entities.Bancario;

namespace AppExampleKafka.Domain.Repositories;

public interface ITransacaoRepository
{
    Task AddAsync(Transacao transacao, OutboxMensagem evento, CancellationToken ct);
    Task<Transacao?> GetByKeyAsync(Guid usuarioId, string chave, CancellationToken ct);
    Task<Transacao?> GetAsync(Guid id, Guid usuarioId, CancellationToken ct);
    Task<(IReadOnlyList<Transacao> Items, int Total)> ListAsync(Guid usuarioId, int page, int size, CancellationToken ct);
}
