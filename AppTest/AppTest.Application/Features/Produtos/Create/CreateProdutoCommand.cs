using AppTest.Application.DTOs.Produtos;
using MediatR;

namespace AppTest.Application.Features.Produtos.Create;

/// <summary>Cria um novo produto vinculado a uma categoria.</summary>
public record CreateProdutoCommand(
    string Nome,
    string? Descricao,
    decimal Preco,
    Guid CategoriaId) : IRequest<ProdutoDto>;
