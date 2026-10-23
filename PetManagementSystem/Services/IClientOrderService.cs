using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

/// <summary>Đơn hàng, thanh toán, hóa đơn của chính người dùng. Đơn không thuộc người dùng được coi như không tồn tại.</summary>
public interface IClientOrderService
{
    Task<MyOrderListViewModel> GetOrdersAsync(int userId, MyOrdersQuery query);
    Task<MyOrderDetailsViewModel?> GetOrderAsync(int userId, int orderId);

    /// <summary>Khách chỉ được hủy khi đơn còn chờ xác nhận; hoàn lại tồn kho và đóng giao dịch đang chờ.</summary>
    Task<ServiceResult> CancelAsync(int userId, int orderId);

    Task<MyInvoiceViewModel?> GetInvoiceAsync(int userId, int orderId);
}
