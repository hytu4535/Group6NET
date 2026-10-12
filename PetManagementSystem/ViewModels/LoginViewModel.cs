using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels;

public class LoginViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }
}
