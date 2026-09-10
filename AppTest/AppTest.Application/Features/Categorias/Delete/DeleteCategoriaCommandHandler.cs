using AppTest.Application.Common.Exceptions;
using AppTest.Domain.Repositories;
using MediatR;

namespace AppTest.Application.Features.Categorias.Delete;

public class DeleteCategoriaCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoriaCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await unitOfWork.Categorias.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Categoria não encontrada.");

        if (await unitOfWork.Produtos.AnyByCategoryAsync(categoria.Id, cancellationToken))
            throw new ConflictException("Não é possível excluir uma categoria com produtos vinculados.");

        unitOfWork.Categorias.Remove(categoria);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
