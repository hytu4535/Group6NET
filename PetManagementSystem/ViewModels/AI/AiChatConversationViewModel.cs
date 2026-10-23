namespace PetManagementSystem.ViewModels.AI;

public class AiChatConversationViewModel
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string UserMessage { get; init; } = string.Empty;
    public string AiMessage { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
