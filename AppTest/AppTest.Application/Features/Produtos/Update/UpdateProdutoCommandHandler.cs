using AppTest.Application.Common.Exceptions;
using AppTest.Application.DTOs.Produtos;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Produtos.Update;

public class UpdateProdutoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateProdutoCommand, ProdutoDto>
{
    public async Task<ProdutoDto> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await unitOfWork.Produtos.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Produto não encontrado.");

        if (!await unitOfWork.Categorias.ExistsAsync(request.CategoriaId, cancellationToken))
            throw new NotFoundException("Categoria não encontrada.");

        produto.Update(request.Nome.Trim(), request.Descricao?.Trim(), request.Preco, request.CategoriaId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarrega para refletir a categoria (possivelmente alterada) na resposta.
        var updated = await unitOfWork.Produtos.GetByIdAsync(produto.Id, cancellationToken);
        return mapper.Map<ProdutoDto>(updated);
    }
}
