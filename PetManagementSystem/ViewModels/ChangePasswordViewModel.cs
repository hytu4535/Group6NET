using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập lại mật khẩu mới là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}