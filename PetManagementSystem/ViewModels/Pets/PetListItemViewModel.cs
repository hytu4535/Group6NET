namespace PetManagementSystem.ViewModels.Pets;

public class PetListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string? Breed { get; init; }
    public string Gender { get; init; } = string.Empty;
    public decimal? WeightKg { get; init; }
    public string? AvatarUrl { get; init; }
}
