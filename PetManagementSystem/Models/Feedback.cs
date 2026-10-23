namespace PetManagementSystem.Models;

public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending / resolved
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
}