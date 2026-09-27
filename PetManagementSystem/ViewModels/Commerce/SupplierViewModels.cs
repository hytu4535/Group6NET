using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Commerce;

public class SupplierRowViewModel
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int ReceiptCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class SupplierListViewModel : PagedListViewModel<SupplierRowViewModel>
{
    public string? Search { get; set; }
}

public class SupplierUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
    [StringLength(150, ErrorMessage = "Tên nhà cung cấp tối đa 150 ký tự")]
    public string SupplierName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
    [RegularExpression(@"^\+?[0-9][0-9\s.\-]{7,14}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    // Cột email trong DB là UNIQUE nhưng cho phép NULL; SQL Server chỉ cho phép 1 dòng NULL
    // trong cột UNIQUE nên email được bắt buộc nhập.
    [Required(ErrorMessage = "Email là bắt buộc")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
    public string? Address { get; set; }
}
