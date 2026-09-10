using AppTest.Application.DTOs.Auth;
using MediatR;

namespace AppTest.Application.Features.Auth.Refresh;

/// <summary>Rotaciona um refresh token válido: revoga o antigo e emite um novo par de tokens.</summary>
public record RefreshCommand(string RefreshToken) : IRequest<AuthResult>;
