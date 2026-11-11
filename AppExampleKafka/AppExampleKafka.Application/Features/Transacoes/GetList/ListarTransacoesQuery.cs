using AppExampleKafka.Application.DTOs.Common;
using AppExampleKafka.Application.DTOs.Transacoes;
using MediatR;

namespace AppExampleKafka.Application.Features.Transacoes.GetList;

public record ListarTransacoesQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<TransacaoDto>>;
