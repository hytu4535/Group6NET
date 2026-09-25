namespace PetManagementSystem.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? AppointmentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "system";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
