using System.ComponentModel.DataAnnotations;
using PetManagementSystem.Helpers;

namespace PetManagementSystem.ViewModels.Shop;

/// <summary>Dữ liệu khách hàng nhập khi đặt hàng. Giá, tổng tiền và trạng thái luôn do server quyết định.</summary>
public class CheckoutInputModel
{
    /// <summary>Số điện thoại Việt Nam: 10-11 chữ số bắt đầu bằng 0, hoặc dạng +84 theo sau 9-10 chữ số.</summary>
    public const string PhonePattern = @"^(?:0[0-9]{9,10}|\+84[0-9]{9,10})$";
    public const string PhoneError = "Số điện thoại phải gồm 10-11 chữ số (bắt đầu bằng 0 hoặc +84)";

    private string _phone = string.Empty;

    [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
    [StringLength(150, ErrorMessage = "Họ tên tối đa 150 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Tự bỏ khoảng trắng/dấu chấm/gạch ngang khi nhận dữ liệu để người dùng gõ "0912 345 678" vẫn hợp lệ.</summary>
    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [RegularExpression(PhonePattern, ErrorMessage = PhoneError)]
    public string Phone
    {
        get => _phone;
        set => _phone = value.NormalizePhone();
    }

    [Required(ErrorMessage = "Địa chỉ nhận hàng là bắt buộc")]
    [StringLength(255, ErrorMessage = "Địa chỉ nhận hàng tối đa 255 ký tự")]
    public string ShippingAddress { get; set; } = string.Empty;

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

public class SavedAddressViewModel
{
    public string Address { get; set; } = string.Empty;
    /// <summary>Nguồn của địa chỉ: "Hồ sơ" hoặc "Đã dùng".</summary>
    public string Source { get; set; } = string.Empty;
}

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();
    public CheckoutInputModel Input { get; set; } = new();
    public List<PaymentOptionViewModel> PaymentOptions { get; set; } = [];
    /// <summary>Địa chỉ có sẵn để chọn nhanh: địa chỉ trong hồ sơ và các địa chỉ đã giao gần đây.</summary>
    public List<SavedAddressViewModel> SavedAddresses { get; set; } = [];
}
