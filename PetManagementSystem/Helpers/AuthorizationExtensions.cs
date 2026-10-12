using Microsoft.AspNetCore.Authorization;
using PetManagementSystem.Models;
namespace PetManagementSystem.Helpers;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        var permissionCodes = new[]
        {
            "users.view", "users.create", "users.update", "users.delete",
            "roles.view", "roles.create", "roles.update", "roles.delete",
            "permissions.view", "permissions.create", "permissions.update", "permissions.delete",
            "staff.view", "staff.create", "staff.update", "staff.delete",
            "veterinarians.view", "veterinarians.create", "veterinarians.update", "veterinarians.delete",
            "pets.view", "pets.create", "pets.update", "pets.delete",
            "pet_images.view", "pet_images.create", "pet_images.update", "pet_images.delete",
            "appointments.view", "appointments.create", "appointments.update", "appointments.delete",
            "services.view", "services.create", "services.update", "services.delete",
            "service_packages.view", "service_packages.create", "service_packages.update", "service_packages.delete",
            "pet_packages.view", "pet_packages.create", "pet_packages.update", "pet_packages.delete",
            "pet_health_records.view", "pet_health_records.create", "pet_health_records.update", "pet_health_records.delete",
            "vaccination_records.view", "vaccination_records.create", "vaccination_records.update", "vaccination_records.delete",
            "medical_prescriptions.view", "medical_prescriptions.create", "medical_prescriptions.update", "medical_prescriptions.delete",
            "feedbacks.view", "feedbacks.create", "feedbacks.update", "feedbacks.delete",
            "notifications.view", "notifications.create", "notifications.update", "notifications.delete",
            "chats.view", "chats.create", "chats.update", "chats.delete",
            "categories.view", "categories.create", "categories.update", "categories.delete",
            "suppliers.view", "suppliers.create", "suppliers.update", "suppliers.delete",
            "products.view", "products.create", "products.update", "products.delete",
            "import_receipts.view", "import_receipts.create", "import_receipts.update", "import_receipts.delete",
            "carts.view", "carts.create", "carts.update", "carts.delete",
            "orders.view", "orders.create", "orders.update", "orders.delete",
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
