namespace PetManagementSystem.ViewModels.Pets;

public class PetDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string? Breed { get; init; }
    public string Gender { get; init; } = string.Empty;
    public decimal? WeightKg { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<PetImageViewModel> Images { get; init; } = [];
}
