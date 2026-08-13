using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Tedas.Shuttle.Application.Common;

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
            var (statusCode, title, code, detail) = exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    "Validation failed.",
                    "VALIDATION_ERROR",
                    string.Join(" ", validationException.Errors.Select(error => error.ErrorMessage))),
                BusinessConflictException conflictException => (
                    StatusCodes.Status409Conflict,
                    conflictException.Message,
                    conflictException.Code,
                    (string?)null),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Beklenmeyen bir hata olustu.",
                    "UNEXPECTED_ERROR",
                    environment.IsDevelopment() ? exception.Message : null)
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled API exception.");
            }
            else
            {
                logger.LogWarning("Handled API exception. Code: {Code}. Message: {Message}", code, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["code"] = code;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
