using AppExampleKafka.Application.DTOs.Usuarios;
using AppExampleKafka.Application.DTOs.Auth;
using MediatR;

namespace AppExampleKafka.Application.Features.Auth.Register;

public record RegisterCommand(string Nome, string Email, string Password, CadastroBancarioDto? Cadastro = null) : IRequest<AuthResult>;
