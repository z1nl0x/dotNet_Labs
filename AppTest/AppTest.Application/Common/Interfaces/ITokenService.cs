using AppTest.Domain.Entities.Usuarios;

namespace AppTest.Application.Common.Interfaces;

/// <summary>Geração de tokens (JWT de acesso e refresh token) e hashing do refresh.</summary>
public interface ITokenService
{
    /// <summary>Gera o JWT de acesso (curta duração) com as claims do usuário.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Usuario usuario);

    /// <summary>
    /// Gera um refresh token opaco e aleatório. Retorna o valor bruto (vai para o
    /// cookie), o hash (persistido no banco) e a data de expiração.
    /// </summary>
    (string RawToken, string TokenHash, DateTime ExpiresAtUtc) GenerateRefreshToken();

    /// <summary>Calcula o hash (SHA-256) de um refresh token bruto para consulta no banco.</summary>
    string HashRefreshToken(string rawToken);
}
