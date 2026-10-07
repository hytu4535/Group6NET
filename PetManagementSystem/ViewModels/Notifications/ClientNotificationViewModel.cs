namespace PetManagementSystem.ViewModels.Notifications;

public class ClientNotificationViewModel
{
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
