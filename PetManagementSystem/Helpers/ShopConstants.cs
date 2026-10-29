namespace PetManagementSystem.Helpers;

public static class ProductSorts
{
    public const string Newest = "newest";
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string Name = "name";

    public static readonly string[] All = [Newest, PriceAsc, PriceDesc, Name];

    public static string Label(string? sort) => sort switch
    {
        PriceAsc => "Giá tăng dần",
        PriceDesc => "Giá giảm dần",
        Name => "Tên A-Z",
        _ => "Mới nhất"
    };
}

/// <summary>Các phương thức thanh toán khách hàng thực sự dùng được khi đặt hàng.</summary>
public static class ClientPaymentMethods
{
    // Khách được chọn Tiền mặt / Chuyển khoản / Ví điện tử. Hệ thống chưa tích hợp cổng thanh toán trực tuyến:
    // giao dịch chuyển khoản và ví điện tử được tạo ở trạng thái "pending" và nhân viên xác nhận trong Quản lý thanh toán.
    public static readonly string[] Supported = [PaymentMethods.Cash, PaymentMethods.BankTransfer, PaymentMethods.EWallet];

    public static string Description(string method) => method switch
    {
        PaymentMethods.Cash => "Thanh toán bằng tiền mặt khi nhận hàng (COD).",
        PaymentMethods.BankTransfer => "Chuyển khoản ngân hàng. Thông tin tài khoản và nội dung chuyển khoản hiển thị sau khi đặt hàng.",
        PaymentMethods.EWallet => "Thanh toán qua ví điện tử. Thông tin thanh toán hiển thị sau khi đặt hàng.",
        _ => string.Empty
    };
}
