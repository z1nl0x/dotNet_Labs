using AppTest.Application.Common.Models;
using AppTest.Application.DTOs.Categories;
using AppTest.Application.DTOs.Common;
using MediatR;

namespace AppTest.Application.Features.Categorias.GetList;

/// <summary>Lista as categorias com filtro opcional por nome e paginação.</summary>
public record GetCategoriasQuery(
    string? Nome = null,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize) : IRequest<PagedResult<CategoriaDto>>;
