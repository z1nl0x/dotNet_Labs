using AppExampleKafka.Domain.Entities.Common;
using AppExampleKafka.Domain.Enums;

namespace AppExampleKafka.Domain.Entities.Usuarios;

/// <summary>
/// Representa um usuário autenticável do sistema.
/// </summary>
public class Usuario : BaseEntity
{
    public CadastroBancario? Cadastro { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>Hash da senha (nunca a senha em texto puro).</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.User;

    // Construtor sem parâmetros exigido pelo EF Core.
    private Usuario() { }

    public Usuario(string nome, string email, string passwordHash, UserRole role = UserRole.User, CadastroBancario? cadastro = null)
    {
        Cadastro = cadastro;
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
