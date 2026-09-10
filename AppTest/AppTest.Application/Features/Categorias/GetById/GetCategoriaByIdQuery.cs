using AppTest.Application.DTOs.Categories;
using MediatR;

namespace AppTest.Application.Features.Categorias.GetById;

/// <summary>Consulta uma categoria pelo id.</summary>
public record GetCategoriaByIdQuery(Guid Id) : IRequest<CategoriaDto>;
