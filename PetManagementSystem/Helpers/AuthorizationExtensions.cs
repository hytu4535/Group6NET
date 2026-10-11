using Microsoft.AspNetCore.Authorization;
using PetManagementSystem.Models;
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
            "pets.view", "appointments.view",
            "categories.view", "categories.create", "categories.update", "categories.delete",
            "suppliers.view", "suppliers.create", "suppliers.update", "suppliers.delete",
            "products.view", "products.create", "products.update", "products.delete",
            "import_receipts.view", "import_receipts.create", "import_receipts.update", "import_receipts.delete",
            "carts.view", "carts.delete",
            "orders.view", "orders.create", "orders.update",
            "payments.view", "payments.create", "payments.update", "payments.delete",
            "invoices.view", "invoices.create", "invoices.update"
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
