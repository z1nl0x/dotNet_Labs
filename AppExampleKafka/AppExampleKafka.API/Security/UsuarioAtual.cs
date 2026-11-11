using System.Security.Claims;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Application.Common.Interfaces;

namespace AppExampleKafka.API.Security;

public sealed class UsuarioAtual(IHttpContextAccessor accessor) : IUsuarioAtual
{
    public Guid Id => accessor.HttpContext?.User.Identity?.IsAuthenticated == true &&
        Guid.TryParse(accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id != Guid.Empty
            ? id : throw new UnauthorizedException("Identidade autenticada inválida.");
}
