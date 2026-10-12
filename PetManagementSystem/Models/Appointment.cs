using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetManagementSystem.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
    public int PetId { get; set; }

    public int UserId { get; set; }

    public int? VetId { get; set; }

    [NotMapped]
    public int? ServicePackageId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày hẹn.")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giờ hẹn.")]
    public TimeSpan AppointmentTime { get; set; }

    public string? Reason { get; set; }

    public byte Status { get; set; } = 1;

    [NotMapped]
    public string? CancelledBy { get; set; }

    [NotMapped]
    public string? CancelReason { get; set; }

    public Pet? Pet { get; set; }
    public User? User { get; set; }
    public Veterinarian? Veterinarian { get; set; }

    [NotMapped]
    public Veterinarian? Vet { get => Veterinarian; set => Veterinarian = value; }

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}
