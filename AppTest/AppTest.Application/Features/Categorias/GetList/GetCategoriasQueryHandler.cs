using AppTest.Application.DTOs.Categories;
using AppTest.Application.DTOs.Common;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Categorias.GetList;

public class GetCategoriasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetCategoriasQuery, PagedResult<CategoriaDto>>
{
    public async Task<PagedResult<CategoriaDto>> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var (categorias, totalCount) = await unitOfWork.Categorias.ListAsync(
            request.Nome, request.PageNumber, request.PageSize, cancellationToken);

        var items = mapper.Map<IReadOnlyList<CategoriaDto>>(categorias);

        return new PagedResult<CategoriaDto>(items, request.PageNumber, request.PageSize, totalCount);
    }
}
