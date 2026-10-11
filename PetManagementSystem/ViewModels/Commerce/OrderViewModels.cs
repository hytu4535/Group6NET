using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Commerce;

public class OrderRowViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class OrderListViewModel : PagedListViewModel<OrderRowViewModel>
{
    public string? Search { get; set; }
    public string? Status { get; set; }

    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    public DateTime? To { get; set; }
}

public class OrderItemRowViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

public class OrderItemListViewModel : PagedListViewModel<OrderItemRowViewModel>
{
    public int? OrderId { get; set; }
    public string? Search { get; set; }
}

public class OrderDetailsViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal PaidAmount { get; set; }
    public bool CanEdit { get; set; }
    public List<string> NextStatuses { get; set; } = [];
    public int? InvoiceId { get; set; }
    public List<OrderItemRowViewModel> Items { get; set; } = [];
    public List<PaymentRowViewModel> Payments { get; set; } = [];
}

public class OrderLineInputViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sản phẩm cho từng dòng")]
    public int ProductId { get; set; }

    [Range(1, 100000, ErrorMessage = "Số lượng phải từ 1 đến 100.000")]
    public int Quantity { get; set; } = 1;
}

public class OrderCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khách hàng")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Địa chỉ giao hàng tối đa 255 ký tự")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
    [RegularExpression(@"^\+?[0-9][0-9\s.\-]{7,14}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    public string? Note { get; set; }

    public List<OrderLineInputViewModel> Lines { get; set; } = [];

    public List<UserOptionViewModel> UserOptions { get; set; } = [];
    public List<ProductOptionViewModel> ProductOptions { get; set; } = [];
}

public class OrderEditViewModel
{
    public int? Id { get; set; }

    /// <summary>Chỉ để hiển thị, không cho sửa khách hàng của đơn.</summary>
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Địa chỉ giao hàng tối đa 255 ký tự")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
    [RegularExpression(@"^\+?[0-9][0-9\s.\-]{7,14}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    public string? Note { get; set; }
}
