using System.Text.Json;
using AuthService.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Middleware
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
            catch (UserAlreadyExistException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status400BadRequest
                }));
            }
            catch (IncorrectLoginOrPasswordException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status401Unauthorized
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
            catch (SecurityTokenException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    status = StatusCodes.Status401Unauthorized
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