using AgroMonitor.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace AgroMonitor.API.Exceptions;

/// <summary>
/// Tratador global de exceções. Converte exceções em respostas padronizadas
/// no formato ProblemDetails (RFC 7807), com o status HTTP adequado.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        var (statusCode, title, detail) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            DomainException e => (StatusCodes.Status400BadRequest, "Não foi possível concluir a operação", e.Message),
            KeyNotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
            InvalidOperationException e => (StatusCodes.Status409Conflict, "Conflito", e.Message),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito",
                "A operação viola uma restrição do banco de dados (registro relacionado ou valor duplicado)."),
            OracleException => (StatusCodes.Status502BadGateway, "Banco indisponível",
                "Não foi possível comunicar com o banco de dados."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };
    }
}
