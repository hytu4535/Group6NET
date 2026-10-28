using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels;

public class CreatePetViewModel
{
    [Range(0, int.MaxValue)]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thú cưng.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Tên thú cưng không được chỉ chứa khoảng trắng.")]
    [StringLength(100, ErrorMessage = "Tên thú cưng không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loài thú cưng.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Loài thú cưng không được chỉ chứa khoảng trắng.")]
    [StringLength(50, ErrorMessage = "Loài thú cưng không được vượt quá 50 ký tự.")]
    public string Species { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giống thú cưng.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "Giống thú cưng không được chỉ chứa khoảng trắng.")]
    [StringLength(80, ErrorMessage = "Giống thú cưng không được vượt quá 80 ký tự.")]
    public string? Breed { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    public string Gender { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "500", ErrorMessage = "Cân nặng phải lớn hơn 0 và không quá 500 kg.")]
    public decimal? WeightKg { get; set; }

    [RecognitionImageFile]
    public IFormFile? AvatarFile { get; init; }
}
