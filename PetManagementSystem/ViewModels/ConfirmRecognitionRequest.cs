using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels;

public sealed class ConfirmRecognitionRequest
{
    [Required(ErrorMessage = "Vui lòng chọn ảnh thú cưng.")]
    [RecognitionImageFile]
    public IFormFile File { get; init; } = null!;

    [Range(1, int.MaxValue, ErrorMessage = "Mã thú cưng không hợp lệ.")]
    public int PetId { get; init; }

    [Range(0d, 1d, ErrorMessage = "Độ tương đồng phải nằm trong khoảng từ 0 đến 1.")]
    [FiniteNumber(ErrorMessage = "Độ tương đồng không hợp lệ.")]
    public double? Similarity { get; init; }
}
