using AppTest.Application.Common.Models;
using AppTest.Application.DTOs.Common;
using AppTest.Application.DTOs.Produtos;
using MediatR;

namespace AppTest.Application.Features.Produtos.GetList;

/// <summary>Lista os produtos com filtro opcional por nome e paginação.</summary>
public record GetProdutosQuery(
    string? Nome = null,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize) : IRequest<PagedResult<ProdutoDto>>;
