using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Identity;

public class VeterinarianUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Bác sĩ người dùng là bắt buộc")]
    public int UserId { get; set; }

    [StringLength(100)]
    [RegularExpression("^[a-zA-Z0-9À-ỹ\\s.,'-]{2,100}$", ErrorMessage = "Chuyên môn không hợp lệ")]
    public string? Specialty { get; set; }

    [StringLength(50)]
    [RegularExpression("^[A-Za-z0-9-]{3,50}$", ErrorMessage = "Số chứng chỉ không hợp lệ")]
    public string? LicenseNo { get; set; }

    [Range(0, 80, ErrorMessage = "Số năm kinh nghiệm không hợp lệ")]
    public int? YearsOfExperience { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(active|inactive)$", ErrorMessage = "Trạng thái chỉ nhận active hoặc inactive")]
    public string Status { get; set; } = "active";

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];
}
