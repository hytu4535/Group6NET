using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetManagementSystem.ViewModels.Commerce;

public class ImportReceiptRowViewModel
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
    public string? Note { get; set; }
}

public class ImportReceiptListViewModel : PagedListViewModel<ImportReceiptRowViewModel>
{
    public string? Search { get; set; }
    public int? SupplierId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    public IEnumerable<SelectListItem> SupplierOptions { get; set; } = [];
}

public class ImportLineInputViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm cho từng dòng")]
    public int ProductId { get; set; }

    [Range(1, 1000000, ErrorMessage = "Số lượng phải từ 1 đến 1.000.000")]
    public int Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Giá nhập phải từ 0 trở lên")]
    public decimal ImportPrice { get; set; }
}

public class ImportReceiptCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhà cung cấp")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Ngày nhập là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime ReceiptDate { get; set; } = DateTime.Now;

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    public string? Note { get; set; }

    public List<ImportLineInputViewModel> Lines { get; set; } = [];

    public IEnumerable<SelectListItem> SupplierOptions { get; set; } = [];
    public List<ProductOptionViewModel> ProductOptions { get; set; } = [];
}

public class ImportReceiptEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhà cung cấp")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Ngày nhập là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime ReceiptDate { get; set; }

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    public string? Note { get; set; }

    public IEnumerable<SelectListItem> SupplierOptions { get; set; } = [];
}

public class ImportReceiptLineRowViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class ImportReceiptDetailsViewModel
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public List<ImportReceiptLineRowViewModel> Lines { get; set; } = [];
}

public class ImportDetailRowViewModel
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class ImportDetailListViewModel : PagedListViewModel<ImportDetailRowViewModel>
{
    public int? ReceiptId { get; set; }
    public string? Search { get; set; }
}
