using Microsoft.AspNetCore.Authorization;

namespace PetManagementSystem.Helpers;

public class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}
