using AppTest.Application.DTOs.Auth;
using MediatR;

namespace AppTest.Application.Features.Auth.Login;

/// <summary>Autentica um usuário por e-mail/senha e emite o par de tokens.</summary>
public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
