using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoaFacil.Api.Infrastructure;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            BadHttpRequestException badRequest => (badRequest.StatusCode,
                "Requisição inválida",
                "O corpo da requisição está malformado ou contém valores inválidos."),
            DbUpdateException => (StatusCodes.Status409Conflict,
                "Conflito ao salvar dados",
                "A operação viola uma restrição do banco de dados (ex.: registro duplicado)."),
            OperationCanceledException => (499,
                "Requisição cancelada",
                "A requisição foi cancelada pelo cliente."),
            _ => (StatusCodes.Status500InternalServerError,
                "Erro interno no servidor",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Falha tratada ({StatusCode}) em {Method} {Path}", statusCode, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}
