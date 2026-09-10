using AppTest.Application.DTOs.Common;
using AppTest.Application.DTOs.Produtos;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Produtos.GetList;

public class GetProdutosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetProdutosQuery, PagedResult<ProdutoDto>>
{
    public async Task<PagedResult<ProdutoDto>> Handle(GetProdutosQuery request, CancellationToken cancellationToken)
    {
        var (produtos, totalCount) = await unitOfWork.Produtos.ListAsync(
            request.Nome, request.PageNumber, request.PageSize, cancellationToken);

        var items = mapper.Map<IReadOnlyList<ProdutoDto>>(produtos);

        return new PagedResult<ProdutoDto>(items, request.PageNumber, request.PageSize, totalCount);
    }
}
