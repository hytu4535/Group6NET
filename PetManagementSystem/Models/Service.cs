using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.Models;

public class Service
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
    [StringLength(255, ErrorMessage = "Tên dịch vụ không được vượt quá 255 ký tự.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ.")]
    [Range(0, 1000000000, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thời lượng dịch vụ.")]
    [Range(1, 10000, ErrorMessage = "Thời lượng dịch vụ phải lớn hơn 0.")]
    public int DurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public byte Status { get; set; } = 1;
    public DateTime? CreatedAt { get; set; }

    public ICollection<ServicePackageService> ServicePackageServices { get; set; } = new List<ServicePackageService>();
}
