namespace PetManagementSystem.Models;

public class Veterinarian
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNo { get; set; }
    public int? YearsOfExperience { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}
