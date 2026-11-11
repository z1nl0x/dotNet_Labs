using AppExampleKafka.Application.Features.Transacoes.Create;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Infraestructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Tests;

public class OutboxTests
{
    [Fact]
    public async Task Falha_no_broker_preserva_pendente_e_retry_usa_mesmo_EventId()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await TransferTests.ContasAsync(f);
        await f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 1, "outbox"));
        var publisher = new TestPublisher { Falhar = true };
        var dispatcher = new OutboxDispatcher(f.Db, publisher);
        await Assert.ThrowsAsync<IOException>(() => dispatcher.PublicarProximaAsync(default));
        Assert.Null((await f.Db.Outbox.AsNoTracking().SingleAsync()).PublicadoEm);
        publisher.Falhar = false;
        Assert.True(await dispatcher.PublicarProximaAsync(default));
        var stored = await f.Db.Outbox.AsNoTracking().SingleAsync();
        Assert.NotNull(stored.PublicadoEm);
        Assert.Equal(new[] { stored.Id, stored.Id }, publisher.Ids);
        Assert.False(await dispatcher.PublicarProximaAsync(default));
    }
}
public sealed class TestPublisher : IEventoPublisher
{
    public bool Falhar { get; set; }
    public List<Guid> Ids { get; } = [];
    public Task PublicarAsync(Guid eventId, Guid transacaoId, string payload, CancellationToken ct)
    {
        Ids.Add(eventId);
        if (Falhar) throw new IOException("broker indisponível");
        return Task.CompletedTask;
    }
}
