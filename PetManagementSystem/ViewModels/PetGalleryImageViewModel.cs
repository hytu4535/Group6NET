namespace PetManagementSystem.ViewModels;

public sealed class PetGalleryImageViewModel
{
    public int Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsAvatar { get; init; }
}
