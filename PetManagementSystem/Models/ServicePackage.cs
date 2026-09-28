using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.Models;

public class ServicePackage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên gói dịch vụ.")]
    [StringLength(255, ErrorMessage = "Tên gói không được vượt quá 255 ký tự.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá gói dịch vụ.")]
    [Range(0, 1000000000, ErrorMessage = "Giá gói phải lớn hơn hoặc bằng 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thời lượng gói dịch vụ.")]
    [Range(1, 10000, ErrorMessage = "Thời lượng gói phải lớn hơn 0.")]
    public int DurationMinutes { get; set; }

    public byte Status { get; set; } = 1;
    public DateTime? CreatedAt { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thời hạn gói dịch vụ.")]
    [Range(1, 3650, ErrorMessage = "Thời hạn gói phải từ 1 ngày trở lên.")]
    public int ValidityDays { get; set; }

    public ICollection<ServicePackageService> ServicePackageServices { get; set; } = new List<ServicePackageService>();
    public ICollection<PetPackage> PetPackages { get; set; } = new List<PetPackage>();
}
