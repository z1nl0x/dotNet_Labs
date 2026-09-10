using System.Net;
using AppTest.Application.Common.Exceptions;

namespace AppTest.API.Middleware;

/// <summary>
/// Middleware que captura exceções da aplicação e devolve uma resposta
/// ProblemDetails com o HTTP status apropriado.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await WriteProblemAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string detail)
    {
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsJsonAsync(new
        {
            status = (int)status,
            title = status.ToString(),
            detail
        });
    }
}
