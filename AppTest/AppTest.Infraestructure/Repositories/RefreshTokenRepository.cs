using AppTest.Domain.Entities;
using AppTest.Domain.Repositories;
using AppTest.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppTest.Infraestructure.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) =>
        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
}
