using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^(0|\+84)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại phải là số di động Việt Nam hợp lệ (10 số hoặc dạng +84)")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}