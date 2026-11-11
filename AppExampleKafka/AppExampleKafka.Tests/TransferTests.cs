using System.Text.Json;
using AppExampleKafka.Application.Features.Transacoes.Create;
using AppExampleKafka.Application.Features.Transacoes.GetById;
using AppExampleKafka.Application.Features.Transacoes.GetList;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Tests;

public class TransferTests
{
    public static async Task<(ContaBancaria Origem, ContaBancaria Destino)> ContasAsync(BankingFixture f)
    {
        var outro = new Usuario("Bia", "bia@example.com", "hash");
        f.Db.Users.Add(outro);
        var origem = new ContaBancaria(f.User.Id);
        var destino = new ContaBancaria(outro.Id);
        f.Db.Contas.AddRange(origem, destino);
        // Aporte apenas no arranjo de testes, nunca um endpoint ou método de crédito público.
        f.Db.Entry(origem).Property(c => c.Saldo).CurrentValue = 100m;
        await f.Db.SaveChangesAsync();
        return (origem, destino);
    }

    [Theory]
    [InlineData(0, "ok")]
    [InlineData(-1, "ok")]
    [InlineData(0.001, "ok")]
    [InlineData(1, "")]
    [InlineData(1, " ")]
    public async Task Requisicao_invalida_retorna_400_sem_mutacoes(decimal valor, string chave)
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.BadRequestException>(() =>
            f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, valor, chave)));
        Assert.Equal(100m, origem.Saldo);
        Assert.Empty(await f.Db.Outbox.ToListAsync());
    }

    [Fact]
    public async Task Saldo_insuficiente_retorna_conflito_e_nao_grava_nada()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.ConflictException>(() => f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 101m, "sem-saldo")));
        Assert.Equal(100m, (await f.Db.Contas.SingleAsync(c => c.Id == origem.Id)).Saldo);
        Assert.Equal(0m, (await f.Db.Contas.SingleAsync(c => c.Id == destino.Id)).Saldo);
        Assert.Empty(await f.Db.Transacoes.ToListAsync());
        Assert.Empty(await f.Db.Outbox.ToListAsync());
    }

    [Fact]
    public async Task Historico_visivel_somente_para_os_titulares_envolvidos()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        var transacao = await f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 1m, "historico"));
        f.User.Id = destino.UsuarioId;
        Assert.Equal(transacao, await f.Sender.Send(new ObterTransacaoQuery(transacao.Id)));
        Assert.Single((await f.Sender.Send(new ListarTransacoesQuery())).Items);
        f.User.Id = Guid.NewGuid();
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.NotFoundException>(() => f.Sender.Send(new ObterTransacaoQuery(transacao.Id)));
        Assert.Empty((await f.Sender.Send(new ListarTransacoesQuery())).Items);
    }

    [Fact]
    public async Task Chave_repetida_retorna_original_e_conflita_se_payload_mudar()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        var command = new CriarTransacaoCommand(origem.Id, destino.Id, 25m, "pedido-123");
        var primeiro = await f.Sender.Send(command);
        var repetido = await f.Sender.Send(command);
        Assert.Equal(primeiro, repetido);
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.ConflictException>(() => f.Sender.Send(command with { Valor = 26m }));
        Assert.Equal(75m, (await f.Db.Contas.SingleAsync(c => c.Id == origem.Id)).Saldo);
        Assert.Single(await f.Db.Transacoes.ToListAsync());
        Assert.Single(await f.Db.Outbox.ToListAsync());
    }

    [Fact]
    public async Task Nao_pode_transferir_saldo_de_outro_titular()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        f.User.Id = destino.UsuarioId;
        await Assert.ThrowsAsync<AppExampleKafka.Application.Common.Exceptions.NotFoundException>(() =>
            f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 25m, "roubo")));
        Assert.Empty(await f.Db.Transacoes.ToListAsync());
        Assert.Empty(await f.Db.Outbox.ToListAsync());
        Assert.Equal(100m, (await f.Db.Contas.SingleAsync(c => c.Id == origem.Id)).Saldo);
    }

    [Fact]
    public async Task Transferencia_CQRS_debita_credita_e_grava_evento_na_mesma_UoW()
    {
        await using var f = new BankingFixture();
        var (origem, destino) = await ContasAsync(f);
        var transacao = await f.Sender.Send(new CriarTransacaoCommand(origem.Id, destino.Id, 25.50m, "teste-1"));
        f.Db.ChangeTracker.Clear();
        Assert.Equal(74.50m, (await f.Db.Contas.SingleAsync(c => c.Id == origem.Id)).Saldo);
        Assert.Equal(25.50m, (await f.Db.Contas.SingleAsync(c => c.Id == destino.Id)).Saldo);
        Assert.Equal(transacao.Id, (await f.Db.Transacoes.SingleAsync()).Id);
        var evento = await f.Db.Outbox.SingleAsync();
        Assert.Null(evento.PublicadoEm);
        using var payload = JsonDocument.Parse(evento.Payload);
        Assert.Equal(evento.Id, payload.RootElement.GetProperty("EventId").GetGuid());
        Assert.Equal(transacao.Id, payload.RootElement.GetProperty("TransacaoId").GetGuid());
        Assert.Equal(1, payload.RootElement.GetProperty("Versao").GetInt32());
        Assert.Equal("TransacaoRealizada", payload.RootElement.GetProperty("Tipo").GetString());
        Assert.Equal(origem.Id, payload.RootElement.GetProperty("ContaOrigemId").GetGuid());
        Assert.Equal(destino.Id, payload.RootElement.GetProperty("ContaDestinoId").GetGuid());
        Assert.Equal(25.50m, payload.RootElement.GetProperty("Valor").GetDecimal());
        Assert.Equal("BRL", payload.RootElement.GetProperty("Moeda").GetString());
        Assert.Equal(transacao.CriadoEm, payload.RootElement.GetProperty("OcorridoEm").GetDateTime());
        Assert.Equal(["ContaDestinoId", "ContaOrigemId", "EventId", "Moeda", "OcorridoEm", "Tipo", "TransacaoId", "Valor", "Versao"],
            payload.RootElement.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.DoesNotContain("example.com", evento.Payload);
    }
}
