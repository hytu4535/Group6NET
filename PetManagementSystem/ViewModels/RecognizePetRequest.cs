using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels;

public sealed class RecognizePetRequest
{
    [Required(ErrorMessage = "Vui lòng chọn ảnh thú cưng.")]
    [RecognitionImageFile]
    public IFormFile File { get; init; } = null!;
}
