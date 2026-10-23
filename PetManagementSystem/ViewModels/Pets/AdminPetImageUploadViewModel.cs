using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels.Pets;

public class AdminPetImageUploadViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thú cưng.")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ảnh.")]
    public IFormFile? Image { get; set; }

    public bool SetAsAvatar { get; set; }
}
