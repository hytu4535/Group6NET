using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.Models;

public class MedicalPrescription
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn hồ sơ sức khỏe.")]
    public int RecordId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên thuốc.")]
    [StringLength(255)]
    public string MedicationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập liều dùng.")]
    [StringLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tần suất dùng.")]
    [StringLength(100)]
    public string Frequency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số ngày dùng thuốc.")]
    [Range(1, 365, ErrorMessage = "Số ngày dùng phải lớn hơn 0.")]
    public int DurationDays { get; set; }

    public PetHealthRecord? PetHealthRecord { get; set; }
}
