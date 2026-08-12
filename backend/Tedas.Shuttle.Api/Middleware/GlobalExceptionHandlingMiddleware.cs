using Microsoft.AspNetCore.Mvc;

namespace Tedas.Shuttle.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception.");

            var problemDetails = new ProblemDetails
            {
                Title = "Beklenmeyen bir hata olustu.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = environment.IsDevelopment() ? exception.Message : null,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["code"] = "UNEXPECTED_ERROR";

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
