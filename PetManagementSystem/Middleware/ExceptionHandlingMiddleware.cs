using PetManagementSystem.Models;

namespace PetManagementSystem.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception at path: {Path}", context.Request.Path);

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(
                    "Đã xảy ra lỗi hệ thống.", "Vui lòng thử lại sau."));
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("<h2>Đã có lỗi xảy ra.</h2><p>Vui lòng thử lại sau.</p>");
        }
    }
}
