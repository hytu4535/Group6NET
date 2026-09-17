using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Identity;

public class StaffUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nhân sự người dùng là bắt buộc")]
    public int UserId { get; set; }

    [StringLength(60)]
    [RegularExpression("^[a-zA-Z0-9À-ỹ\\s.,'-]{2,60}$", ErrorMessage = "Vị trí không hợp lệ")]
    public string? Position { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDate { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(active|inactive)$", ErrorMessage = "Trạng thái chỉ nhận active hoặc inactive")]
    public string Status { get; set; } = "active";

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];
}
