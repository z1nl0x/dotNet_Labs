using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Application.Features.Auth.Login;
using AppExampleKafka.Application.Features.Auth.Logout;
using AppExampleKafka.Application.Features.Auth.Refresh;
using AppExampleKafka.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppExampleKafka.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Nome do cookie HTTP-only que transporta o refresh token.</summary>
    private const string RefreshCookieName = "refreshToken";

    /// <summary>Registra um novo usuário (role padrão: User) e retorna um access token JWT.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        SetRefreshCookie(result);
        return Ok(result.Response);
    }

    /// <summary>Autentica um usuário e retorna um access token JWT (refresh token vai em cookie HTTP-only).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        SetRefreshCookie(result);
        return Ok(result.Response);
    }

    /// <summary>
    /// Renova o access token usando o refresh token do cookie HTTP-only.
    /// O refresh token é rotacionado: o antigo é revogado e um novo é emitido.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        var result = await sender.Send(new RefreshCommand(refreshToken ?? string.Empty), cancellationToken);
        SetRefreshCookie(result);
        return Ok(result.Response);
    }

    /// <summary>Revoga o refresh token atual e apaga o cookie (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        await sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        Response.Cookies.Delete(RefreshCookieName, BuildCookieOptions());
        return NoContent();
    }

    /// <summary>Endpoint protegido: acessível a qualquer usuário autenticado.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        Ok(await sender.Send(new AppExampleKafka.Application.Features.Auth.Perfil.ObterMeuPerfilQuery(), ct));

    /// <summary>Endpoint protegido: apenas usuários com a role Admin.</summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() => Ok(new { Message = "Você é um administrador." });

    /// <summary>Grava o refresh token em um cookie HTTP-only, seguro e SameSite=Strict.</summary>
    private void SetRefreshCookie(AuthResult result)
    {
        var options = BuildCookieOptions();
        options.Expires = result.RefreshTokenExpiresAtUtc;
        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, options);
    }

    private CookieOptions BuildCookieOptions() => new()
    {
        HttpOnly = true,                 // inacessível a JavaScript (mitiga XSS)
        Secure = Request.IsHttps,        // só trafega sob HTTPS (true em produção)
        SameSite = SameSiteMode.Strict,  // mitiga CSRF
        IsEssential = true,
        Path = "/"
    };
}
