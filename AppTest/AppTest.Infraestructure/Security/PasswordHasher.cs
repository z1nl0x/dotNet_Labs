using AppTest.Application.Common.Interfaces;

namespace AppTest.Infraestructure.Security;

/// <summary>Implementação de hashing de senha usando BCrypt.</summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
