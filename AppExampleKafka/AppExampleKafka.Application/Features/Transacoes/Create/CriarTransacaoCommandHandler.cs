using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Application.DTOs.Transacoes;
using AppExampleKafka.Application.Contracts.Events;
using MediatR;
using System.Text.Json;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Transacoes.Create;

public class CriarTransacaoCommandHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<CriarTransacaoCommand, TransacaoDto>
{
    public Task<TransacaoDto> Handle(CriarTransacaoCommand request, CancellationToken ct) => uow.ExecutarTransacaoAsync(async () =>
    {
        var anterior = await uow.Transacoes.GetByKeyAsync(usuario.Id, request.ChaveIdempotencia, ct);
        if (anterior is not null)
        {
            if (anterior.ContaOrigemId != request.ContaOrigemId || anterior.ContaDestinoId != request.ContaDestinoId || anterior.Valor != request.Valor)
                throw new ConflictException("Chave de idempotência já utilizada com outro conteúdo.");
            return mapper.Map<TransacaoDto>(anterior);
        }
        var origem = await uow.Contas.GetAsync(request.ContaOrigemId, ct) ?? throw new NotFoundException("Conta não encontrada.");
        if (origem.UsuarioId != usuario.Id) throw new NotFoundException("Conta não encontrada.");
        var destino = await uow.Contas.GetAsync(request.ContaDestinoId, ct) ?? throw new NotFoundException("Conta não encontrada.");
        try { origem.TransferirPara(destino, request.Valor); }
        catch (InvalidOperationException ex) { throw new ConflictException(ex.Message); }
        catch (ArgumentException ex) { throw new BadRequestException(ex.Message); }
        var transacao = new Transacao(usuario.Id, origem, destino, request.Valor, request.ChaveIdempotencia);
        var eventId = Guid.NewGuid();
        var evento = new TransacaoRealizadaV1(eventId, 1, "TransacaoRealizada", transacao.Id, origem.Id, destino.Id, transacao.Valor, transacao.Moeda, transacao.CriadoEm);
        await uow.Transacoes.AddAsync(transacao, new OutboxMensagem(eventId, transacao.Id, JsonSerializer.Serialize(evento)), ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<TransacaoDto>(transacao);
    }, ct);
}
