using AppExampleKafka.Application.DTOs.Common;
using AppExampleKafka.Application.DTOs.Contas;
using MediatR;

namespace AppExampleKafka.Application.Features.Contas.GetList;

public record ListarContasQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ContaBancariaDto>>;
