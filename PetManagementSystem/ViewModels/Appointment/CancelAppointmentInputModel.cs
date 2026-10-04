using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Appointment;

public class CancelAppointmentInputModel
{
    [Required]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do hủy lịch hẹn.")]
    [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
    public string CancelReason { get; set; } = string.Empty;
}
