using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Identity;

public class UserUpsertViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
    [RegularExpression("^[a-zA-Z0-9._]{4,40}$", ErrorMessage = "Tên tài khoản 4-40 ký tự, chỉ gồm chữ, số, dấu chấm hoặc gạch dưới")]
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [RegularExpression("^(?=.*[a-z])(?=.*\\d).{8,64}$", ErrorMessage = "Mật khẩu 8-64 ký tự, cần có chữ thường và số")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [RegularExpression("^[a-zA-ZÀ-ỹ\\s]{2,60}$", ErrorMessage = "Họ tên chỉ gồm chữ và khoảng trắng, 2-60 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression("^(0|\\+84)(3|5|7|8|9)\\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 09xxxxxxxx hoặc +849xxxxxxxx)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(active|inactive)$", ErrorMessage = "Trạng thái chỉ nhận active hoặc inactive")]
    public string Status { get; set; } = "active";

    public IEnumerable<SelectListItem> RoleOptions { get; set; } = [];

    public bool IsEdit => Id.HasValue;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsEdit)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("Mật khẩu là bắt buộc khi tạo mới", [nameof(Password)]);
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                yield return new ValidationResult("Xác nhận mật khẩu là bắt buộc", [nameof(ConfirmPassword)]);
            }
        }

        if (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult("Mật khẩu xác nhận không khớp", [nameof(ConfirmPassword)]);
            }
        }
    }
}
