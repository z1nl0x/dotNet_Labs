using AppExampleKafka.Application.DTOs.Transacoes;
using MediatR;

namespace AppExampleKafka.Application.Features.Transacoes.GetById;

public record ObterTransacaoQuery(Guid Id) : IRequest<TransacaoDto>;
