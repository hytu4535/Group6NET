namespace PetManagementSystem.ViewModels.Pets;

public class AdminPetImageListItemViewModel
{
    public int Id { get; init; }
    public int PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public string? OwnerName { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsAvatar { get; init; }
    public DateTime UploadedAt { get; init; }
}
