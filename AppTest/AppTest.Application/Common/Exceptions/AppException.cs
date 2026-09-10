using System.Net;

namespace AppTest.Application.Common.Exceptions;

/// <summary>
/// Exceção base da aplicação que carrega um HTTP status code,
/// permitindo tratamento centralizado no middleware da API.
/// </summary>
public abstract class AppException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

/// <summary>Requisição inválida (400).</summary>
public sealed class BadRequestException(string message) : AppException(HttpStatusCode.BadRequest, message);

/// <summary>Recurso não encontrado (404).</summary>
public sealed class NotFoundException(string message) : AppException(HttpStatusCode.NotFound, message);

/// <summary>Conflito de estado, ex.: e-mail já cadastrado (409).</summary>
public sealed class ConflictException(string message) : AppException(HttpStatusCode.Conflict, message);

/// <summary>Credenciais inválidas / não autenticado (401).</summary>
public sealed class UnauthorizedException(string message) : AppException(HttpStatusCode.Unauthorized, message);
