using AppTest.Domain.Entities;

namespace AppTest.Domain.Repositories;

/// <summary>Contrato de acesso a dados para <see cref="RefreshToken"/>.</summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
