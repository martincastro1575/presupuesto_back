using PlanificadorGastos.API.Models.Common;
using System.Net;
using System.Text.Json;

namespace PlanificadorGastos.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            UnauthorizedAccessException _ => new ExceptionResponse(
                HttpStatusCode.Unauthorized,
                "No autorizado",
                "No tiene permisos para realizar esta acción"),

            KeyNotFoundException _ => new ExceptionResponse(
                HttpStatusCode.NotFound,
                "Recurso no encontrado",
                exception.Message),

            ArgumentException _ => new ExceptionResponse(
                HttpStatusCode.BadRequest,
                "Solicitud inválida",
                exception.Message),

            InvalidOperationException _ => new ExceptionResponse(
                HttpStatusCode.BadRequest,
                "Operación inválida",
                exception.Message),

            _ => new ExceptionResponse(
                HttpStatusCode.InternalServerError,
                "Error interno del servidor",
                "Ha ocurrido un error inesperado. Por favor, intente nuevamente.")
        };

        context.Response.StatusCode = (int)response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(ApiResponse.Fail(response.Message, [response.Details]), 
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(jsonResponse);
    }

    private record ExceptionResponse(HttpStatusCode StatusCode, string Message, string Details);
}
