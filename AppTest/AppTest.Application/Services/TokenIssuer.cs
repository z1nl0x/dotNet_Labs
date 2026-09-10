using AppTest.Application.Common.Interfaces;
using AppTest.Application.DTOs.Auth;
using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Usuarios;
using AppTest.Domain.Repositories;
using AutoMapper;

namespace AppTest.Application.Services;

/// <inheritdoc cref="ITokenIssuer"/>
internal sealed class TokenIssuer(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IMapper mapper) : ITokenIssuer
{
    public async Task<AuthResult> IssueAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        var (accessToken, accessExpires) = tokenService.GenerateAccessToken(usuario);
        var (rawRefresh, refreshHash, refreshExpires) = tokenService.GenerateRefreshToken();

        await unitOfWork.RefreshTokens.AddAsync(new RefreshToken(usuario.Id, refreshHash, refreshExpires), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(accessToken, accessExpires, mapper.Map<UserDto>(usuario));
        return new AuthResult(response, rawRefresh, refreshExpires);
    }
}
