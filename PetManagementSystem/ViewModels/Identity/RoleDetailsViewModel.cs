using PetManagementSystem.Models;

namespace PetManagementSystem.ViewModels.Identity;

public class RoleDetailsViewModel
{
    public Role Role { get; set; } = new();
    public RolePermissionsViewModel PermissionMatrix { get; set; } = new();
}
