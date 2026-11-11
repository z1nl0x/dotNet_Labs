using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Domain.Entities;
using AppExampleKafka.Domain.Entities.Usuarios;
using AppExampleKafka.Domain.Repositories;
using AutoMapper;

namespace AppExampleKafka.Application.Services;

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
