using AppTest.Application.Common.Exceptions;
using AppTest.Application.Common.Interfaces;
using AppTest.Application.DTOs.Auth;
using AppTest.Domain.Entities;
using AppTest.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace AppTest.Application.Features.Auth.Refresh;

public class RefreshCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IMapper mapper) : IRequestHandler<RefreshCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new UnauthorizedException("Refresh token ausente.");

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await unitOfWork.RefreshTokens.GetByTokenHashAsync(hash, cancellationToken);

        if (stored is null || !stored.IsActive)
            throw new UnauthorizedException("Refresh token inválido ou expirado.");

        var user = await unitOfWork.Users.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Usuário não encontrado.");

        // Rotação: o token usado é revogado e um novo é emitido.
        var (accessToken, accessExpires) = tokenService.GenerateAccessToken(user);
        var (rawRefresh, refreshHash, refreshExpires) = tokenService.GenerateRefreshToken();

        stored.Revoke(replacedByTokenHash: refreshHash);
        await unitOfWork.RefreshTokens.AddAsync(new RefreshToken(user.Id, refreshHash, refreshExpires), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(accessToken, accessExpires, mapper.Map<UserDto>(user));
        return new AuthResult(response, rawRefresh, refreshExpires);
    }
}
