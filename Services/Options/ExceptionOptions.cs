using Microsoft.AspNetCore.Http;

namespace Services.Options
{
    public class ExceptionOptions
    {
        public Dictionary<Type, int> ExceptionHandlers { get; set; } = new()
        {
            { typeof(KeyNotFoundException), StatusCodes.Status404NotFound },
            { typeof(ArgumentException), StatusCodes.Status400BadRequest },
            { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized }
        };
    }
}
