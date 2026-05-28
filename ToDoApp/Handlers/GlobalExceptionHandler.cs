using Microsoft.AspNetCore.Diagnostics;

namespace ToDoApp.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly Dictionary<Type, int> _exceptionHandlers = new()
        {
            { typeof(KeyNotFoundException), StatusCodes.Status404NotFound },
            { typeof(ArgumentException), StatusCodes.Status400BadRequest },
            { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized }
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var exceptionType = exception.GetType();

            if (_exceptionHandlers.TryGetValue(exceptionType, out var handler))
            {
                httpContext.Response.StatusCode = handler;
                await httpContext.Response.WriteAsJsonAsync(new { Error = exception.Message }, cancellationToken);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred." }, cancellationToken);
            }

            return true;
        }
    }
}
