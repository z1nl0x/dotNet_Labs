using AppExampleKafka.Domain.Entities;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
}
