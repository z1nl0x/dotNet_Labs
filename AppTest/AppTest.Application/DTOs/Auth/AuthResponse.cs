namespace AppTest.Application.DTOs.Auth;

/// <summary>Resposta retornada após autenticação/registro bem-sucedido.</summary>
public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    UserDto User);
