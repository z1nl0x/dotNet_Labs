using AppTest.Application.Common.Exceptions;
using AppTest.Domain.Repositories;
using MediatR;

namespace AppTest.Application.Features.Produtos.Delete;

public class DeleteProdutoCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProdutoCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await unitOfWork.Produtos.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Produto não encontrado.");

        unitOfWork.Produtos.Remove(produto);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
