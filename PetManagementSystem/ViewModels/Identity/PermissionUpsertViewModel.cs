using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Identity;

public class PermissionUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Mã quyền là bắt buộc")]
    [RegularExpression("^[a-z0-9_]+\\.[a-z0-9_]+$", ErrorMessage = "Mã quyền theo dạng module.action, ví dụ users.view")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên quyền là bắt buộc")]
    [StringLength(40)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Module là bắt buộc")]
    [StringLength(40)]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Module chỉ gồm chữ, số, gạch dưới")]
    public string Module { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
