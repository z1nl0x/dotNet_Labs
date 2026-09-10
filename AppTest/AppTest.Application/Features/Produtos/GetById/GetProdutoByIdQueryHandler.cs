using AppTest.Application.Common.Exceptions;
using AppTest.Application.DTOs.Produtos;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Produtos.GetById;

public class GetProdutoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetProdutoByIdQuery, ProdutoDto>
{
    public async Task<ProdutoDto> Handle(GetProdutoByIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await unitOfWork.Produtos.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Produto não encontrado.");

        return mapper.Map<ProdutoDto>(produto);
    }
}
