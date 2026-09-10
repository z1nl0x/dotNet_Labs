namespace AppTest.Application.DTOs.Auth;

/// <summary>Representação pública (sem dados sensíveis) de um usuário.</summary>
public record UserDto(Guid Id, string Nome, string Email, string Role);
