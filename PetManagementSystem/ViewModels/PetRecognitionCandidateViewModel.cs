namespace PetManagementSystem.ViewModels;

public class PetRecognitionCandidateViewModel
{
    public string PetId { get; set; } = string.Empty;
    public string? PetImageUrl { get; set; }
    public double Similarity { get; set; }

    public int DatabasePetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }

    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public string? OwnerEmail { get; set; }
}