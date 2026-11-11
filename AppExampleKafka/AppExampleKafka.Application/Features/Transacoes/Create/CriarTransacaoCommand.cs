using AppExampleKafka.Application.DTOs.Transacoes;
using MediatR;

namespace AppExampleKafka.Application.Features.Transacoes.Create;

public record CriarTransacaoCommand(Guid ContaOrigemId, Guid ContaDestinoId, decimal Valor, string ChaveIdempotencia) : IRequest<TransacaoDto>;
