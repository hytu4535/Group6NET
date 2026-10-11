using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Commerce;

public class InvoiceRowViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string IssuedByName { get; set; } = string.Empty;
}

public class InvoiceListViewModel : PagedListViewModel<InvoiceRowViewModel>
{
    public string? Search { get; set; }

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }
}

public class InvoiceUpsertViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn hàng")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Ngày lập hóa đơn là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    /// <summary>Tiền thuế (đã bao gồm trong tổng tiền hóa đơn).</summary>
    [Range(typeof(decimal), "0", "99999999999999", ErrorMessage = "Tiền thuế phải từ 0 trở lên")]
    public decimal TaxAmount { get; set; }

    /// <summary>Chỉ để hiển thị: tổng tiền của hóa đơn lấy theo đơn hàng.</summary>
    public decimal TotalAmount { get; set; }

    public string OrderText { get; set; } = string.Empty;

    public List<OrderOptionViewModel> OrderOptions { get; set; } = [];
}

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount => TotalAmount - TaxAmount;
    public string IssuedByName { get; set; } = string.Empty;
    public List<OrderItemRowViewModel> Items { get; set; } = [];
}
