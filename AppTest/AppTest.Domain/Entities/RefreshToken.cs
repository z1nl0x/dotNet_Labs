namespace AppTest.Domain.Entities;

/// <summary>
/// Representa um refresh token persistido. Guarda apenas o HASH do token
/// (nunca o valor em texto puro) e suporta rotação/revogação.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid UserId { get; private set; }

    /// <summary>Hash (SHA-256) do token — o valor bruto vive apenas no cookie do cliente.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>Hash do token que substituiu este (trilha de rotação).</summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>Ativo = não revogado e ainda não expirado.</summary>
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    // Construtor sem parâmetros exigido pelo EF Core.
    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Revoga o token. Opcionalmente registra qual token o substituiu.</summary>
    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
