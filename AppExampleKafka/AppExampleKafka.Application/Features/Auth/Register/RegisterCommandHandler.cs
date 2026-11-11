using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Domain.Entities.Usuarios;
using AppExampleKafka.Domain.Enums;
using AppExampleKafka.Domain.Repositories;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Auth.Register;

public class RegisterCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    IMapper mapper) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (request.Cadastro is null) throw new BadRequestException("Cadastro bancário obrigatório.");
        if (await unitOfWork.Users.DocumentoExistsAsync(request.Cadastro.Documento, cancellationToken))
            throw new ConflictException("Documento já cadastrado.");

        if (await unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("Já existe um usuário com este e-mail.");

        var usuario = new Usuario(
            nome: request.Nome.Trim(),
            email: email,
            passwordHash: passwordHasher.Hash(request.Password),
            role: UserRole.User,
            cadastro: mapper.Map<CadastroBancario>(request.Cadastro));

        await unitOfWork.Users.AddAsync(usuario, cancellationToken);

        return await tokenIssuer.IssueAsync(usuario, cancellationToken);
    }
}
