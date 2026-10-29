namespace PetManagementSystem.ViewModels.Pets;

public class PetImageViewModel
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsAvatar { get; init; }
    public DateTime CreatedAt { get; init; }
}
