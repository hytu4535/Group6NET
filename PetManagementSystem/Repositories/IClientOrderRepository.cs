using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

/// <summary>Đơn hàng, thanh toán và hóa đơn của khách hàng. Mọi truy vấn đều ràng buộc theo chủ đơn (user_id).</summary>
public interface IClientOrderRepository
{
    Task<IRepositoryTransaction> BeginTransactionAsync();

    /// <summary>Trừ tồn kho nguyên tử: chỉ thành công khi sản phẩm đang bán và còn đủ số lượng.</summary>
    Task<bool> TryReserveStockAsync(int productId, int quantity);

    Task ReleaseStockAsync(int productId, int quantity);

    /// <summary>Các địa chỉ giao hàng khác nhau trong những đơn gần đây của người dùng (mới nhất trước).</summary>
    Task<List<string>> GetRecentShippingAddressesAsync(int userId, int take);

    Task AddOrderAsync(Order order);
    Task AddPaymentAsync(Payment payment);

    Task<PagedResult<Order>> SearchOwnedAsync(int userId, string? status, DateTime? from, DateTime? to, int page, int pageSize);

    /// <summary>Đơn của chính người dùng kèm sản phẩm, thanh toán và hóa đơn (không theo dõi thay đổi).</summary>
    Task<Order?> GetOwnedDetailsAsync(int userId, int orderId);

    /// <summary>Hóa đơn của đơn hàng thuộc người dùng, kèm khách hàng và các dòng sản phẩm.</summary>
    Task<Invoice?> GetOwnedInvoiceAsync(int userId, int orderId);

    /// <summary>Chuyển pending -> cancelled nguyên tử. False nếu đơn không thuộc người dùng hoặc không còn pending.</summary>
    Task<bool> TryCancelPendingAsync(int userId, int orderId);

    Task<bool> HasCompletedPaymentAsync(int orderId);

    /// <summary>Chuyển các giao dịch đang chờ của đơn sang failed (đơn đã hủy nên sẽ không thu tiền).</summary>
    Task FailPendingPaymentsAsync(int orderId);
}
