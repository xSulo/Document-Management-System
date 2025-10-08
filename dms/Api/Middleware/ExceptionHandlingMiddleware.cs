using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using dms.Api.Exceptions; 

namespace dms.Api.Middleware
{
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception intercepted by global middleware");

            ProblemDetails problem = ex switch
            {
                ValidationException valEx => new ValidationProblemDetails(
                    valEx.Errors
                        .GroupBy(e => e.PropertyName ?? string.Empty)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        ))
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path
                },

                KeyNotFoundException => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "Resource not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },

                QueueUnavailableException => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/503",
                    Title = "Queue unavailable",
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                },

                _ => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                }
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
