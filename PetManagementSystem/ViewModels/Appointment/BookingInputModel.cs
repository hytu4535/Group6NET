using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Appointment;

public class BookingInputModel
{
    [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ít nhất một dịch vụ.")]
    public List<int> ServiceIds { get; set; } = new();

    public int? ServicePackageId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày hẹn.")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Vui lòng chọn giờ hẹn.")]
    public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

    [StringLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự.")]
    public string? Reason { get; set; }
}
