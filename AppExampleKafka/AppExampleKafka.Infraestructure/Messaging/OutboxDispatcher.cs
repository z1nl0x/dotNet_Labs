using System.Data;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Messaging;

/// <summary>Lock mantido até o ACK e commit. SKIP LOCKED permite vários workers PostgreSQL.</summary>
public class OutboxDispatcher(AppDbContext db, IEventoPublisher publisher)
{
    public async Task<bool> PublicarProximaAsync(CancellationToken ct)
    {
        var isolation = db.Database.IsNpgsql() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable;
        await using var tx = await db.Database.BeginTransactionAsync(isolation, ct);
        try
        {
            OutboxMensagem? mensagem;
            if (db.Database.IsNpgsql())
            {
                var rows = await db.Outbox.FromSqlRaw("SELECT * FROM outbox_mensagens WHERE \"PublicadoEm\" IS NULL ORDER BY \"CriadoEm\", \"Id\" LIMIT 1 FOR UPDATE SKIP LOCKED").ToListAsync(ct);
                mensagem = rows.FirstOrDefault();
            }
            else mensagem = await db.Outbox.Where(e => e.PublicadoEm == null).OrderBy(e => e.CriadoEm).ThenBy(e => e.Id).FirstOrDefaultAsync(ct);
            if (mensagem is null) return false;
            await publisher.PublicarAsync(mensagem.Id, mensagem.TransacaoId, mensagem.Payload, ct);
            mensagem.MarcarPublicado();
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return true;
        }
        finally
        {
            // Falha deixa o registro pendente; DisposeAsync da transação desfaz o mark.
            db.ChangeTracker.Clear();
        }
    }
}
