namespace PetManagementSystem.Models;

public class Pet
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<PetPackage> PetPackages { get; set; } = new List<PetPackage>();
}
