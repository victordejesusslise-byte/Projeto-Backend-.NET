using System.Diagnostics;
using System.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Exceptions;

namespace UsuariosAPI.API.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        ApiResponse<object> response;

        switch (exception)
        {
            case NotFoundException notFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ApiResponse<object>(false, notFound.Message, null, TraceId: traceId);
                break;

            case ConflictException conflict:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new ApiResponse<object>(false, conflict.Message, null, TraceId: traceId);
                break;

            case Application.Exceptions.ValidationException validation:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response = new ApiResponse<object>(
                    false,
                    validation.Message,
                    null,
                    validation.Erros,
                    traceId);
                break;

            case DbUpdateException dbUpdate when EhViolacaoDeUnicidade(dbUpdate):
                _logger.LogWarning(
                    dbUpdate,
                    "Conflito de unicidade ao persistir usuário. TraceId: {TraceId}",
                    traceId);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new ApiResponse<object>(
                    false,
                    "Já existe um usuário com os dados únicos informados.",
                    null,
                    TraceId: traceId);
                break;

            case BadHttpRequestException badRequest:
                _logger.LogWarning(
                    badRequest,
                    "Requisição HTTP inválida. TraceId: {TraceId}",
                    traceId);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ApiResponse<object>(
                    false,
                    "Não foi possível processar a requisição.",
                    null,
                    new[] { "A requisição HTTP é inválida." },
                    traceId);
                break;

            case BusinessException business:
                context.Response.StatusCode = business.StatusCode;
                response = new ApiResponse<object>(false, business.Message, null, TraceId: traceId);
                break;

            default:
                _logger.LogError(
                    exception,
                    "Erro interno não tratado. TraceId: {TraceId}",
                    traceId);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new ApiResponse<object>(
                    false,
                    "Ocorreu um erro interno. Tente novamente mais tarde.",
                    null,
                    TraceId: traceId,
                    Debug: _environment.IsDevelopment()
                        ? new DebugResponse(
                            exception.GetType().FullName ?? exception.GetType().Name,
                            exception.Message,
                            exception.StackTrace)
                        : null);
                break;
        }

        await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }

    private static bool EhViolacaoDeUnicidade(DbUpdateException exception)
        => exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
