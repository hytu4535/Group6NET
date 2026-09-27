using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetManagementSystem.Models;

public class PetHealthRecord
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
    public int PetId { get; set; }

    public int? AppointmentId { get; set; }

    public int? VetId { get; set; }

    [NotMapped]
    public Veterinarian? Vet { get => Veterinarian; set => Veterinarian = value; }

    [Range(0, 999.99, ErrorMessage = "Cân nặng không hợp lệ.")]
    public decimal? WeightKg { get; set; }

    [Range(30, 45, ErrorMessage = "Nhiệt độ không hợp lệ.")]
    public decimal? Temperature { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập chẩn đoán.")]
    public string? Diagnosis { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập phương pháp điều trị.")]
    public string? Treatment { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
    public DateTime VisitDate { get; set; } = DateTime.Today;

    public Pet? Pet { get; set; }
    public Veterinarian? Veterinarian { get; set; }
    public ICollection<MedicalPrescription> MedicalPrescriptions { get; set; } = new List<MedicalPrescription>();
}
