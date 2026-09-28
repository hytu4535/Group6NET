using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetManagementSystem.Models;

public class AppointmentService
{
    public int AppointmentId { get; set; }
    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; } = null!;

    public int ServiceId { get; set; }
    [ForeignKey("ServiceId")]
    public virtual Service Service { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }
}
