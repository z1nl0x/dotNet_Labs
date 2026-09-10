using AppTest.Application.DTOs.Produtos;
using MediatR;

namespace AppTest.Application.Features.Produtos.Update;

/// <summary>Atualiza um produto existente.</summary>
public record UpdateProdutoCommand(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    Guid CategoriaId) : IRequest<ProdutoDto>;
