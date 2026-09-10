using MediatR;

namespace AppTest.Application.Features.Categorias.Delete;

/// <summary>Remove uma categoria. Falha se houver produtos vinculados.</summary>
public record DeleteCategoriaCommand(Guid Id) : IRequest<Unit>;
