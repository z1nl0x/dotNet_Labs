using AppTest.Application.DTOs.Produtos;
using MediatR;

namespace AppTest.Application.Features.Produtos.GetById;

/// <summary>Consulta um produto pelo id.</summary>
public record GetProdutoByIdQuery(Guid Id) : IRequest<ProdutoDto>;
