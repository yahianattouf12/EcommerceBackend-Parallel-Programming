using System.Net;
using System.Text.Json;

namespace ECommerceBackend.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var error = new
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
