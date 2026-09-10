using MediatR;

namespace AppTest.Application.Features.Auth.Logout;

/// <summary>Revoga o refresh token informado (logout). Idempotente.</summary>
public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
