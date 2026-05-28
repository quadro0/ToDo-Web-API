using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Services.Options;

namespace ToDoApp.Handlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IOptions<ExceptionOptions> exceptionOptions) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var exceptionType = exception.GetType();

            if (exceptionOptions.Value.ExceptionHandlers.TryGetValue(exceptionType, out var handler))
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
