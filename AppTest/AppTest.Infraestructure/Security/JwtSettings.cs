namespace AppTest.Infraestructure.Security;

/// <summary>Opções de configuração do JWT (seção "Jwt" no appsettings).</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Validade do access token (JWT). Curta por design.</summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>Validade do refresh token, em dias.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
