using GamePicker.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace GamePicker.Filters
{
    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                var response = ApiResponse.CreateError("Validation failed", errors, 400);
                
                context.Result = new BadRequestObjectResult(response);
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                var statusCode = objectResult.StatusCode ?? 200;
                
                if (objectResult.Value is ApiResponse || objectResult.Value.GetType().IsGenericType && 
                    objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
                {
                    return;
                }

                var apiResponseType = typeof(ApiResponse<>).MakeGenericType(objectResult.Value.GetType());
                var successMethod = apiResponseType.GetMethod("CreateSuccess");
                var apiResponse = successMethod!.Invoke(null, new[] { objectResult.Value, (string?)null });

                context.Result = new ObjectResult(apiResponse)
                {
                    StatusCode = statusCode
                };
            }
        }
    }
}
