using AppTest.Application.Common.Exceptions;
using AppTest.Application.Common.Interfaces;
using AppTest.Application.DTOs.Auth;
using AppTest.Domain.Entities.Usuarios;
using AppTest.Domain.Enums;
using AppTest.Domain.Repositories;
using MediatR;

namespace AppTest.Application.Features.Auth.Register;

public class RegisterCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("Já existe um usuário com este e-mail.");

        var usuario = new Usuario(
            nome: request.Nome.Trim(),
            email: email,
            passwordHash: passwordHasher.Hash(request.Password),
            role: UserRole.User);

        await unitOfWork.Users.AddAsync(usuario, cancellationToken);

        return await tokenIssuer.IssueAsync(usuario, cancellationToken);
    }
}
