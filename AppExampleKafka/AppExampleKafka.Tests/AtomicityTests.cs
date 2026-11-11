using AppExampleKafka.Application.Features.Transacoes.Create;
using AppExampleKafka.Domain.Entities.Bancario;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AppExampleKafka.Tests;

public class AtomicityTests
{
    [Fact]
    public async Task Falha_apos_SaveChanges_antes_de_commit_desfaz_debito_credito_historico_evento()
    {
        await using var f = new BankingFixture(new FailureAfterSave());
        var (origem, destino) = await TransferTests.ContasAsync(f);
        await Assert.ThrowsAsync<IOException>(() => f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 10m, "rollback")));
        f.Db.ChangeTracker.Clear();
        Assert.Equal(100m, (await f.Db.Contas.SingleAsync(c => c.Id == origem.Id)).Saldo);
        Assert.Equal(0m, (await f.Db.Contas.SingleAsync(c => c.Id == destino.Id)).Saldo);
        Assert.Empty(await f.Db.Transacoes.ToListAsync());
        Assert.Empty(await f.Db.Outbox.ToListAsync());
    }
}
public class FailureAfterSave : SaveChangesInterceptor
{
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context!.ChangeTracker.Entries<Transacao>().Any()) throw new IOException("Falha injetada após escrita mas antes do commit.");
        return ValueTask.FromResult(result);
    }
}
