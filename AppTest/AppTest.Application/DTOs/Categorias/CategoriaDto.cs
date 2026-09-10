namespace AppTest.Application.DTOs.Categories;

/// <summary>Representação pública de uma categoria.</summary>
public record CategoriaDto(Guid Id, string Nome, string? Descricao, DateTime CriadoEm, DateTime AtualizadoEm);
