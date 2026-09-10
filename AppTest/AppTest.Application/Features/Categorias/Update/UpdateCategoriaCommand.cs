using AppTest.Application.DTOs.Categories;
using MediatR;

namespace AppTest.Application.Features.Categorias.Update;

/// <summary>Atualiza uma categoria existente.</summary>
public record UpdateCategoriaCommand(Guid Id, string Nome, string? Descricao) : IRequest<CategoriaDto>;
