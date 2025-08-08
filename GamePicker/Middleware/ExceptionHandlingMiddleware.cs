using GamePicker.Application.Common.Exceptions;
using GamePicker.Contracts;
using System.Net;
using System.Text.Json;

namespace GamePicker.Middleware
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = exception switch
            {
                GamePickerException gamePickerEx => ApiResponse.CreateError(
                    gamePickerEx.Message,
                    gamePickerEx is ValidationException validationEx ? validationEx.Errors : null,
                    gamePickerEx.StatusCode),

                ArgumentException => ApiResponse.CreateError("Invalid argument provided", statusCode: 400),
                InvalidOperationException => ApiResponse.CreateError("Invalid operation", statusCode: 400),
                UnauthorizedAccessException => ApiResponse.CreateError("Unauthorized access", statusCode: 401),
                KeyNotFoundException => ApiResponse.CreateError("Resource not found", statusCode: 404),
                TimeoutException => ApiResponse.CreateError("Request timeout", statusCode: 408),
                HttpRequestException => ApiResponse.CreateError("External service error", statusCode: 502),
                TaskCanceledException => ApiResponse.CreateError("Request cancelled", statusCode: 408),
                _ => ApiResponse.CreateError("An unexpected error occurred", statusCode: 500)
            };

            response.StatusCode = apiResponse.StatusCode;

            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            await response.WriteAsync(jsonResponse);
        }
    }
}
