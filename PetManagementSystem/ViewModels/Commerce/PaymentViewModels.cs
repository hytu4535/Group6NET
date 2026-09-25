using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Commerce;

public class PaymentRowViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionCode { get; set; }
}

public class PaymentListViewModel : PagedListViewModel<PaymentRowViewModel>
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Method { get; set; }
}

public class PaymentUpsertViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn hàng")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    [RegularExpression("^(cash|bank_transfer|credit_card|e_wallet)$", ErrorMessage = "Phương thức thanh toán không hợp lệ")]
    public string PaymentMethod { get; set; } = "cash";

    [Range(typeof(decimal), "0.01", "99999999999999", ErrorMessage = "Số tiền phải lớn hơn 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Ngày thanh toán là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [RegularExpression("^(pending|completed|failed|refunded)$", ErrorMessage = "Trạng thái không hợp lệ")]
    public string Status { get; set; } = "pending";

    [StringLength(100, ErrorMessage = "Mã giao dịch tối đa 100 ký tự")]
    [RegularExpression(@"^[A-Za-z0-9._\-]*$", ErrorMessage = "Mã giao dịch chỉ gồm chữ, số và các ký tự . _ -")]
    public string? TransactionCode { get; set; }

    /// <summary>Chỉ để hiển thị khi sửa (không đổi đơn hàng của một giao dịch).</summary>
    public string OrderText { get; set; } = string.Empty;

    public List<OrderOptionViewModel> OrderOptions { get; set; } = [];
}
