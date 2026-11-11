using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Application.DTOs.Common;
using AppExampleKafka.Application.DTOs.Contas;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Contas.GetList;

public class ListarContasQueryHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<ListarContasQuery, PagedResult<ContaBancariaDto>>
{
    public async Task<PagedResult<ContaBancariaDto>> Handle(ListarContasQuery request, CancellationToken ct)
    {
        var (items, total) = await uow.Contas.ListAsync(usuario.Id, request.PageNumber, request.PageSize, ct);
        return new(mapper.Map<List<ContaBancariaDto>>(items), request.PageNumber, request.PageSize, total);
    }
}
