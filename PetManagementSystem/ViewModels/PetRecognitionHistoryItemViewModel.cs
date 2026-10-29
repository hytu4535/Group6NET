namespace PetManagementSystem.ViewModels;

public sealed class PetRecognitionHistoryItemViewModel
{
    public int Id { get; init; }
    public int? PetId { get; init; }
    public string PetName { get; init; } = "Chưa xác định";
    public string PetCode => PetId.HasValue ? $"PET{PetId.Value:000}" : string.Empty;
    public string? PetSpecies { get; init; }
    public string? PetBreed { get; init; }
    public string OwnerName { get; init; } = "Không xác định";
    public string? OwnerPhone { get; init; }
    public string? InputImageUrl { get; init; }
    public string? PetImageUrl { get; init; }
    public decimal? SimilarityScore { get; init; }
    public DateTime RecognizedAt { get; init; }
    public string StaffName { get; init; } = "Không xác định";
    public string? Notes { get; init; }
}
