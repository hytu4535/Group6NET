namespace PetManagementSystem.Models;

public class Staff
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}
