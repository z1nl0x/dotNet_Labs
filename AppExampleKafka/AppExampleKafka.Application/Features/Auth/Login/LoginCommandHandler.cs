using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Domain.Repositories;
using MediatR;

namespace AppExampleKafka.Application.Features.Auth.Login;

public class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("E-mail ou senha inválidos.");

        return await tokenIssuer.IssueAsync(user, cancellationToken);
    }
}
