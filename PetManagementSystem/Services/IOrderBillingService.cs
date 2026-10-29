namespace PetManagementSystem.Services;

/// <summary>Nghiệp vụ nối Đơn hàng -> Thanh toán -> Hóa đơn.</summary>
public interface IOrderBillingService
{
    /// <summary>
    /// Tự lập hóa đơn cho đơn hàng khi đơn chưa hủy, chưa có hóa đơn và các giao dịch đã thanh toán thành công
    /// phủ đủ tổng tiền đơn. Trả về true nếu vừa lập một hóa đơn mới.
    /// </summary>
    Task<bool> EnsureInvoiceAsync(int orderId);
}
