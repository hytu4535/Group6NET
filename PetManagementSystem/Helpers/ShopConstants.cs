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
    // Hệ thống chưa tích hợp cổng thanh toán trực tuyến nên chỉ hỗ trợ thanh toán khi nhận hàng.
    public static readonly string[] Supported = [PaymentMethods.Cash];

    public static string Description(string method) => method switch
    {
        PaymentMethods.Cash => "Thanh toán bằng tiền mặt khi nhận hàng (COD).",
        _ => string.Empty
    };
}
