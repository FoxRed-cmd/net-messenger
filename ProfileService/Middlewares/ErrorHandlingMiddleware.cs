using System.Text.Json;
using ProfileService.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace ProfileService.Middlewares
{
    public class ErrorHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (EmailOrUsernameAlreadyExistsException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status400BadRequest
                }));
            }
            catch (EntityNotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status404NotFound
                }));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status500InternalServerError
                }));
            }
        }
    }
}