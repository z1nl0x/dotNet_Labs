using AppTest.Application.Common.Interfaces;
using AppTest.Domain.Repositories;
using MediatR;

namespace AppTest.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenService tokenService) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unit.Value;

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await unitOfWork.RefreshTokens.GetByTokenHashAsync(hash, cancellationToken);

        if (stored is not null && stored.IsActive)
        {
            stored.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
