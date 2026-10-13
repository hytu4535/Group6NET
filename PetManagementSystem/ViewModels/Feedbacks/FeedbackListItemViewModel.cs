namespace PetManagementSystem.ViewModels.Feedbacks;

public class FeedbackListItemViewModel
{
    public int Id { get; init; }
    public string Comment { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
