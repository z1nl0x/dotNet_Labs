using AppTest.Domain.Entities.Common;
using AppTest.Domain.Enums;

namespace AppTest.Domain.Entities.Usuarios;

/// <summary>
/// Representa um usuário autenticável do sistema.
/// </summary>
public class Usuario : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>Hash da senha (nunca a senha em texto puro).</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.User;

    // Construtor sem parâmetros exigido pelo EF Core.
    private Usuario() { }

    public Usuario(string nome, string email, string passwordHash, UserRole role = UserRole.User)
    {
        Nome = nome;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash) => PasswordHash = newPasswordHash;

    public void ChangeRole(UserRole role) => Role = role;
}
