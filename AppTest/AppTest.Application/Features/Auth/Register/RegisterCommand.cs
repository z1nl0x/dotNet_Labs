using AppTest.Application.DTOs.Auth;
using MediatR;

namespace AppTest.Application.Features.Auth.Register;

/// <summary>Registra um novo usuário (role padrão: User) e emite o par de tokens.</summary>
public record RegisterCommand(string Nome, string Email, string Password) : IRequest<AuthResult>;
