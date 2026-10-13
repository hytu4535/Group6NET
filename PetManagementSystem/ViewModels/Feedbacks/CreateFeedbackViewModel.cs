using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Feedbacks;

public class CreateFeedbackViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
    [StringLength(2000, ErrorMessage = "Phản hồi không được vượt quá 2.000 ký tự.")]
    public string Comment { get; set; } = string.Empty;
}
