namespace PetManagementSystem.Middleware;

public class AuditUserActionMiddleware(
    RequestDelegate next,
    ILogger<AuditUserActionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var username = context.User.Identity.Name ?? "unknown";
            var path = context.Request.Path;
            logger.LogInformation("Audit user action: {User} -> {Path}", username, path);
        }

        await next(context);
    }
}
