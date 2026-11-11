using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Repositories;

public class TransacaoRepository(AppDbContext db) : ITransacaoRepository
{
    private IQueryable<Transacao> Visiveis(Guid usuarioId) => db.Transacoes.AsNoTracking().Where(t =>
        db.Contas.Any(c => c.UsuarioId == usuarioId && (c.Id == t.ContaOrigemId || c.Id == t.ContaDestinoId)));
    public Task<Transacao?> GetAsync(Guid id, Guid usuarioId, CancellationToken ct) => Visiveis(usuarioId).FirstOrDefaultAsync(t => t.Id == id, ct);
    public async Task<(IReadOnlyList<Transacao> Items, int Total)> ListAsync(Guid usuarioId, int page, int size, CancellationToken ct)
    {
        var query = Visiveis(usuarioId);
        return (await query.OrderByDescending(t => t.CriadoEm).ThenBy(t => t.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct), await query.CountAsync(ct));
    }
    public Task<Transacao?> GetByKeyAsync(Guid usuarioId, string chave, CancellationToken ct) =>
        db.Transacoes.AsNoTracking().FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.ChaveIdempotencia == chave, ct);
    public async Task AddAsync(Transacao transacao, OutboxMensagem evento, CancellationToken ct)
    {
        await db.Transacoes.AddAsync(transacao, ct);
        await db.Outbox.AddAsync(evento, ct);
    }
}
