using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IOrderRepository
{
    Task<PagedResult<Order>> SearchAsync(string? search, string? status, DateTime? from, DateTime? to, int page, int pageSize);

    Task<PagedResult<OrderItem>> SearchItemsAsync(int? orderId, string? search, int page, int pageSize);

    /// <summary>Đơn hàng kèm khách hàng, sản phẩm, thanh toán và hóa đơn (có theo dõi thay đổi).</summary>
    Task<Order?> GetByIdWithDetailsAsync(int id);

    /// <summary>Các đơn chưa hủy (kèm khách hàng, thanh toán) để chọn khi tạo thanh toán.</summary>
    Task<List<Order>> GetOrdersForPaymentAsync();

    /// <summary>
    /// Các đơn chưa hủy và chưa có hóa đơn để chọn khi lập hóa đơn.
    /// <paramref name="includeOrderId"/> luôn được đưa vào danh sách nếu có.
    /// </summary>
    Task<List<Order>> GetOrdersForInvoiceAsync(int? includeOrderId = null);

    /// <summary>
    /// Các repository dùng chung một <c>AppDbContext</c> (scoped) nên thay đổi tồn kho của
    /// sản phẩm đã được nạp trước đó sẽ được lưu cùng transaction với đơn hàng.
    /// </summary>
    Task AddAsync(Order order);

    Task UpdateAsync(Order order);
}
