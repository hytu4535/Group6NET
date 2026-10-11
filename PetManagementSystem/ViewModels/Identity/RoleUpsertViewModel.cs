using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Identity;

public class RoleUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(active|inactive)$", ErrorMessage = "Trạng thái chỉ nhận active hoặc inactive")]
    public string Status { get; set; } = "active";

    public List<int> SelectedPermissionIds { get; set; } = [];
    public List<SelectListItem> PermissionOptions { get; set; } = [];
}
