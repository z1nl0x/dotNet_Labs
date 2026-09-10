using AppTest.Application.Common.Exceptions;
using AppTest.Application.Common.Interfaces;
using AppTest.Application.DTOs.Auth;
using AppTest.Domain.Repositories;
using MediatR;

namespace AppTest.Application.Features.Auth.Login;

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
