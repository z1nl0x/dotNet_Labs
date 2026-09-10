using AppTest.Application.DTOs.Categories;
using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Categorias;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Categorias.Create;

public class CreateCategoriaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateCategoriaCommand, CategoriaDto>
{
    public async Task<CategoriaDto> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = new Categoria(request.Nome.Trim(), request.Descricao?.Trim());

        await unitOfWork.Categorias.AddAsync(categoria, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CategoriaDto>(categoria);
    }
}
