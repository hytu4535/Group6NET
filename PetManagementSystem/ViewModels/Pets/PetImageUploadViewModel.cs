using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels.Pets;

public class PetImageUploadViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn ảnh.")]
    public IFormFile? Image { get; set; }

    public bool SetAsAvatar { get; set; }
}
