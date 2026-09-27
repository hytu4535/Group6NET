namespace PetManagementSystem.Helpers;

public static class ProductStatuses
{
    public const string Active = "active";
    public const string Inactive = "inactive";

    public static readonly string[] All = [Active, Inactive];

    public static string Label(string? status) => status switch
    {
        Active => "Đang bán",
        Inactive => "Ngừng bán",
        _ => status ?? "-"
    };

    public static string Badge(string? status) => status == Active ? "text-bg-success" : "text-bg-secondary";
}

public static class CartStatuses
{
    public const string Active = "active";
    public const string CheckedOut = "checked_out";

    public static readonly string[] All = [Active, CheckedOut];

    public static string Label(string? status) => status switch
    {
        Active => "Đang hoạt động",
        CheckedOut => "Đã đặt hàng",
        _ => status ?? "-"
    };

    public static string Badge(string? status) => status == Active ? "text-bg-primary" : "text-bg-secondary";
}

public static class OrderStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Shipping = "shipping";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    public static readonly string[] All = [Pending, Confirmed, Shipping, Completed, Cancelled];

    /// <summary>Các trạng thái được phép chuyển tới từ trạng thái hiện tại.</summary>
    public static IReadOnlyList<string> NextStatuses(string? current) => current switch
    {
        Pending => [Confirmed, Cancelled],
        Confirmed => [Shipping, Cancelled],
        Shipping => [Completed, Cancelled],
        _ => []
    };

    public static bool CanTransition(string? from, string? to) =>
        to is not null && NextStatuses(from).Contains(to);

    /// <summary>Chỉ được sửa thông tin giao hàng khi đơn chưa bắt đầu giao.</summary>
    public static bool CanEdit(string? status) => status is Pending or Confirmed;

    public static string Label(string? status) => status switch
    {
        Pending => "Chờ xác nhận",
        Confirmed => "Đã xác nhận",
        Shipping => "Đang giao",
        Completed => "Hoàn thành",
        Cancelled => "Đã hủy",
        _ => status ?? "-"
    };

    public static string Badge(string? status) => status switch
    {
        Pending => "text-bg-warning",
        Confirmed => "text-bg-info",
        Shipping => "text-bg-primary",
        Completed => "text-bg-success",
        Cancelled => "text-bg-secondary",
        _ => "text-bg-light"
    };
}

public static class PaymentMethods
{
    public const string Cash = "cash";
    public const string BankTransfer = "bank_transfer";
    public const string CreditCard = "credit_card";
    public const string EWallet = "e_wallet";

    public static readonly string[] All = [Cash, BankTransfer, CreditCard, EWallet];

    public static string Label(string? method) => method switch
    {
        Cash => "Tiền mặt",
        BankTransfer => "Chuyển khoản",
        CreditCard => "Thẻ tín dụng",
        EWallet => "Ví điện tử",
        _ => method ?? "-"
    };
}

public static class PaymentStatuses
{
    public const string Pending = "pending";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Refunded = "refunded";

    public static readonly string[] All = [Pending, Completed, Failed, Refunded];

    public static string Label(string? status) => status switch
    {
        Pending => "Chờ thanh toán",
        Completed => "Đã thanh toán",
        Failed => "Thất bại",
        Refunded => "Đã hoàn tiền",
        _ => status ?? "-"
    };

    public static string Badge(string? status) => status switch
    {
        Pending => "text-bg-warning",
        Completed => "text-bg-success",
        Failed => "text-bg-danger",
        Refunded => "text-bg-secondary",
        _ => "text-bg-light"
    };
}
