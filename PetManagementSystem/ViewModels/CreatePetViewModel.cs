using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels;

public class CreatePetViewModel
{
    [Range(0, int.MaxValue)]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thú cưng.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loài thú cưng.")]
    [StringLength(50)]
    public string Species { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
    public string Gender { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "500", ErrorMessage = "Cân nặng phải lớn hơn 0 và không quá 500 kg.")]
    public decimal? WeightKg { get; set; }

    [RecognitionImageFile]
    public IFormFile? AvatarFile { get; init; }
}
