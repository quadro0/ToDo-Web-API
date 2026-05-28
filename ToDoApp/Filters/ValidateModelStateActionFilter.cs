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

                context.Result = new BadRequestObjectResult(new
                {
                    Error = context.ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage).ToList()
                });

                return;
            }

            await next();
        }
    }
}
