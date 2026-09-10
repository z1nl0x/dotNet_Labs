namespace AppTest.Application.Common.Interfaces;

/// <summary>Abstração para hash e verificação de senhas.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
