namespace AppTest.Application.DTOs.Auth;

/// <summary>
/// Resultado interno de um caso de uso de autenticação. Carrega o corpo da
/// resposta (<see cref="AuthResponse"/>, com o access token) e o refresh token
/// bruto, que a camada de API grava em um cookie HTTP-only — nunca no corpo.
/// </summary>
public record AuthResult(
    AuthResponse Response,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
