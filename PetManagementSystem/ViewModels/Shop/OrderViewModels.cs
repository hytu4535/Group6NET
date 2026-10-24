using PetManagementSystem.Helpers;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.ViewModels.Shop;

public class MyOrdersQuery
{
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
}

public class MyOrderRowViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }

    /// <summary>Trạng thái thanh toán tổng hợp; null nếu chưa có giao dịch.</summary>
    public string? PaymentStatus { get; set; }
}

public class MyOrderListViewModel : PagedListViewModel<MyOrderRowViewModel>
{
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public bool HasFilter => !string.IsNullOrEmpty(Status) || From.HasValue || To.HasValue;

    public Dictionary<string, string?> RouteValues => new()
    {
        ["status"] = Status,
        ["from"] = From?.ToString("yyyy-MM-dd"),
        ["to"] = To?.ToString("yyyy-MM-dd")
    };
}

public class MyOrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

public class MyPaymentViewModel
{
    public int Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionCode { get; set; }
}

public class MyOrderDetailsViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverEmail { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);
    public bool CanCancel { get; set; }
    public bool HasInvoice { get; set; }
    public List<MyOrderItemViewModel> Items { get; set; } = [];
    public List<MyPaymentViewModel> Payments { get; set; } = [];
}

public class MyInvoiceViewModel
{
    public int InvoiceId { get; set; }
    public int OrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public List<MyOrderItemViewModel> Items { get; set; } = [];
}
