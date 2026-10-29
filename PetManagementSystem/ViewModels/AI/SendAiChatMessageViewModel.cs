using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.AI;

public class SendAiChatMessageViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập câu hỏi.")]
    [StringLength(2000, ErrorMessage = "Tin nhắn không được vượt quá 2.000 ký tự.")]
    public string Message { get; set; } = string.Empty;
}
