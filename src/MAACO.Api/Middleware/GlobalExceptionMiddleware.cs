using MAACO.Api.Contracts.Common;
using System.Text.Json;

namespace MAACO.Api.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new ApiError(
                "internal_error",
                "Internal Server Error",
                null,
                context.TraceIdentifier);

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
