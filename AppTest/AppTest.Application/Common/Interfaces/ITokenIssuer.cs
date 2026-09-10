using AppTest.Application.DTOs.Auth;
using AppTest.Domain.Entities.Usuarios;

namespace AppTest.Application.Common.Interfaces;

/// <summary>
/// Emite o par access token (JWT) + refresh token para um usuário e persiste o
/// hash do refresh token. Concentra a lógica compartilhada entre os casos de uso
/// de registro e login.
/// </summary>
public interface ITokenIssuer
{
    Task<AuthResult> IssueAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
