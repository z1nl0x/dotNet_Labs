using AppTest.Application.Common.Exceptions;
using AppTest.Application.DTOs.Produtos;
using AppTest.Domain.Entities.Produtos;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Produtos.Create;

public class CreateProdutoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateProdutoCommand, ProdutoDto>
{
    public async Task<ProdutoDto> Handle(CreateProdutoCommand request, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Categorias.ExistsAsync(request.CategoriaId, cancellationToken))
            throw new NotFoundException("Categoria não encontrada.");

        var produto = new Produto(request.Nome.Trim(), request.Descricao?.Trim(), request.Preco, request.CategoriaId);

        await unitOfWork.Produtos.AddAsync(produto, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Recarrega com a navegação de categoria para popular CategoryName na resposta.
        var created = await unitOfWork.Produtos.GetByIdAsync(produto.Id, cancellationToken);
        return mapper.Map<ProdutoDto>(created);
    }
}
