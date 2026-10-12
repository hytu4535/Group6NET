using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels;

public class RegisterViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Xác nhận mật khẩu")]
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải gồm 10 hoặc 11 chữ số và bắt đầu bằng 0")]
    public string Phone { get; set; } = string.Empty;
}
