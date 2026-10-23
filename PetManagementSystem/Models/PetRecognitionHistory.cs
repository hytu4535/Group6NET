namespace PetManagementSystem.Models;

public class PetRecognitionHistory
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int? SelectedPetId { get; set; }

    public string? DetectedSpecies { get; set; }

    public string? DetectedBreed { get; set; }

    public decimal? TopSimilarity { get; set; }

    public int? ExecutionTimeMs { get; set; }

    public DateTime CreatedAt { get; set; }

    public Staff? Staff { get; set; }

    public Pet? SelectedPet { get; set; }
}