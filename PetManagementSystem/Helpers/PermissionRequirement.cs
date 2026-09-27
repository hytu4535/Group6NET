using Microsoft.AspNetCore.Authorization;
using PetManagementSystem.Models;
namespace PetManagementSystem.Helpers;

public class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}
