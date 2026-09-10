using AppTest.Application.Common.Exceptions;
using AppTest.Application.DTOs.Categories;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Categorias.GetById;

public class GetCategoriaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetCategoriaByIdQuery, CategoriaDto>
{
    public async Task<CategoriaDto> Handle(GetCategoriaByIdQuery request, CancellationToken cancellationToken)
    {
        var categoria = await unitOfWork.Categorias.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Categoria não encontrada.");

        return mapper.Map<CategoriaDto>(categoria);
    }
}
