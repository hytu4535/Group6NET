namespace PetManagementSystem.Models;

public class ChatConversation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string UserRequest { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}