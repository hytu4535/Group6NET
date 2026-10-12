using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetManagementSystem.Models;

public class VaccinationRecord
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên vắc-xin.")]
    public string VaccineName { get; set; } = string.Empty;

    public string? BatchNumber { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày tiêm.")]
    public DateTime AdministeredDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng chọn ngày tiêm tiếp theo.")]
    public DateTime NextDueDate { get; set; } = DateTime.Today.AddYears(1);

    public int? VetId { get; set; }

    [NotMapped]
    public Veterinarian? Vet { get => Veterinarian; set => Veterinarian = value; }

    [ForeignKey("PetId")]
    public Pet? Pet { get; set; }

    [ForeignKey("VetId")]
    public Veterinarian? Veterinarian { get; set; }
}
