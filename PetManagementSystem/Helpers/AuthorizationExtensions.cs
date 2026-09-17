using Microsoft.AspNetCore.Authorization;

namespace PetManagementSystem.Helpers;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        var permissionCodes = new[]
        {
            "users.view", "users.create", "users.update",
            "roles.view", "roles.update",
            "permissions.view", "permissions.update",
            "staff.view", "staff.update",
            "veterinarians.view", "veterinarians.update",
            "pets.view", "appointments.view", "orders.view"
        };

        services.AddAuthorization(options =>
        {
            foreach (var code in permissionCodes)
            {
                options.AddPolicy(code, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new PermissionRequirement(code));
                });
            }
        });

        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        return services;
    }
}
