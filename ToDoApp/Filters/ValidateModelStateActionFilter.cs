using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ToDoApp.Filters
{
    public class ValidateModelStateActionFilter(ILogger<ValidateModelStateActionFilter> logger) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                foreach (var error in context.ModelState.Values)
                {
                    logger.LogWarning("Validating error: {Error}", error);
                }

                context.Result = new BadRequestObjectResult(context.ModelState);
                return; 
            }

            var executedContext = await next();

            if (executedContext.Exception != null)
            {
                executedContext.Result = executedContext.Exception switch
                {
                    KeyNotFoundException => new NotFoundObjectResult(executedContext.Exception.Message),
                    ArgumentException => new BadRequestObjectResult(executedContext.Exception.Message),
                    UnauthorizedAccessException => new UnauthorizedObjectResult(executedContext.Exception.Message),

                    _ => new ObjectResult(executedContext.Exception.Message) { StatusCode = 500 }
                };

                executedContext.ExceptionHandled = true;
            }
        }
    }
}
