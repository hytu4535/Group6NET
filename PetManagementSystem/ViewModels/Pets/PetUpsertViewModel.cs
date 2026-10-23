using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels.Pets;

public class PetUpsertViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên thú cưng.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loài thú cưng.")]
    [StringLength(50)]
    public string Species { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Breed { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Giới tính không hợp lệ.")]
    public string Gender { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999.99", ErrorMessage = "Cân nặng phải lớn hơn 0.")]
    public decimal? WeightKg { get; set; }

    public IFormFile? Image { get; set; }
}
