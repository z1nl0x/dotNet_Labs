using AppTest.Application.Common.Exceptions;
using AppTest.Application.DTOs.Categories;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Categorias.Update;

public class UpdateCategoriaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateCategoriaCommand, CategoriaDto>
{
    public async Task<CategoriaDto> Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await unitOfWork.Categorias.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Categoria não encontrada.");

        categoria.Update(request.Nome.Trim(), request.Descricao?.Trim());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CategoriaDto>(categoria);
    }
}
