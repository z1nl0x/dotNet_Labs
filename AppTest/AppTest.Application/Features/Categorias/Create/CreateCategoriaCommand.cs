using AppTest.Application.DTOs.Categories;
using MediatR;

namespace AppTest.Application.Features.Categorias.Create;

/// <summary>Cria uma nova categoria.</summary>
public record CreateCategoriaCommand(string Nome, string? Descricao) : IRequest<CategoriaDto>;
