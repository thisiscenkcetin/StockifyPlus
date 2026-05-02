using Microsoft.AspNetCore.Http;
using StockifyPlus.Exceptions;

namespace StockifyPlus.Middleware
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";

            string message = exception.Message;
            int statusCode = StatusCodes.Status500InternalServerError;

            switch (exception)
            {
                case ValidationException ve:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = $"DoÄŸrulama HatasÄ±: {ve.Message}";
                    break;

                case NotFoundException ne:
                    statusCode = StatusCodes.Status404NotFound;
                    message = $"BulunamadÄ±: {ne.Message}";
                    break;

                case BusinessException be:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = $"Ä°ÅŸ KuralÄ± HatasÄ±: {be.Message}";
                    break;

                default:
                    message = "Sunucu HatasÄ± - LÃ¼tfen sistem yÃ¶neticisine baÅŸvurunuz.";
                    break;
            }

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(new
            {
                error = true,
                message = message,
                statusCode = statusCode,
                timestamp = DateTime.UtcNow
            });
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
