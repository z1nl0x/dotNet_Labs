using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Application.DTOs.Common;
using AppExampleKafka.Application.DTOs.Transacoes;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Transacoes.GetList;

public class ListarTransacoesQueryHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<ListarTransacoesQuery, PagedResult<TransacaoDto>>
{
    public async Task<PagedResult<TransacaoDto>> Handle(ListarTransacoesQuery request, CancellationToken ct)
    {
        var (items, total) = await uow.Transacoes.ListAsync(usuario.Id, request.PageNumber, request.PageSize, ct);
        return new(mapper.Map<List<TransacaoDto>>(items), request.PageNumber, request.PageSize, total);
    }
}
