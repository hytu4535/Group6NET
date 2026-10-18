using System.ComponentModel.DataAnnotations;
using PetManagementSystem.Helpers;

namespace PetManagementSystem.ViewModels.Shop;

/// <summary>Dữ liệu khách hàng nhập khi đặt hàng. Giá, tổng tiền và trạng thái luôn do server quyết định.</summary>
public class CheckoutInputModel
{
    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Địa chỉ giao hàng tối đa 255 ký tự")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
    [RegularExpression(@"^\+?[0-9][0-9\s.\-]{7,14}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Ghi chú tối đa 255 ký tự")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    public string PaymentMethod { get; set; } = PaymentMethods.Cash;
}

public class PaymentOptionViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CheckoutInputModel Input { get; set; } = new();
    public List<PaymentOptionViewModel> PaymentOptions { get; set; } = [];
}
