namespace PetManagementSystem.ViewModels.Identity;

public class RolePermissionsViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleStatus { get; set; } = string.Empty;
    public List<RolePermissionModuleRowViewModel> Rows { get; set; } = [];
    public List<int> SelectedPermissionIds { get; set; } = [];
}

public class RolePermissionModuleRowViewModel
{
    public string Module { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public RolePermissionCellViewModel View { get; set; } = new();
    public RolePermissionCellViewModel Create { get; set; } = new();
    public RolePermissionCellViewModel Update { get; set; } = new();
    public RolePermissionCellViewModel Delete { get; set; } = new();
}

public class RolePermissionCellViewModel
{
    public int PermissionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsGranted { get; set; }
}