using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Commerce;

public class ProductRowViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProductListViewModel : PagedListViewModel<ProductRowViewModel>
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = [];
}

public class ProductUpsertViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "Đơn vị tính tối đa 20 ký tự")]
    public string? Unit { get; set; }

    [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Giá nhập phải từ 0 trở lên")]
    public decimal ImportPrice { get; set; }

    [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Giá bán phải từ 0 trở lên")]
    public decimal SellPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải từ 0 trở lên")]
    public int StockQuantity { get; set; }


    /// <summary>Ảnh hiện tại: link ngoài (http/https) hoặc đường dẫn ảnh đã tải lên (/uploads/products/...).</summary>
    [StringLength(1000, ErrorMessage = "Đường dẫn ảnh tối đa 1000 ký tự")]
    public string? ImageUrl { get; set; }

    /// <summary>Ảnh chọn từ máy; nếu có thì thay thế ảnh hiện tại.</summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>Tích để bỏ ảnh hiện tại (khi không chọn ảnh mới).</summary>
    public bool RemoveImage { get; set; }


    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(active|inactive)$", ErrorMessage = "Trạng thái chỉ nhận active hoặc inactive")]
    public string Status { get; set; } = "active";

    public IEnumerable<SelectListItem> CategoryOptions { get; set; } = [];
}
