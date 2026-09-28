using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.Models;

public class PetPackage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thú cưng.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thú cưng hợp lệ.")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn gói dịch vụ.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn gói dịch vụ hợp lệ.")]
    public int PackageId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
    public DateTime EndDate { get; set; }

    public byte Status { get; set; } = 1;

    public Pet? Pet { get; set; }
    public ServicePackage? ServicePackage { get; set; }
}
