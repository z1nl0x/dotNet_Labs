using MediatR;

namespace AppTest.Application.Features.Produtos.Delete;

/// <summary>Remove um produto.</summary>
public record DeleteProdutoCommand(Guid Id) : IRequest<Unit>;
