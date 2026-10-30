using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IInvoiceRepository
{
    Task<PetManagementSystem.Models.PagedResult<Invoice>> SearchAsync(string? search, DateTime? from, DateTime? to, int page, int pageSize);

    /// <summary>Hóa đơn kèm đơn hàng, khách hàng, sản phẩm và người lập.</summary>
    Task<Invoice?> GetByIdAsync(int id);

    Task<bool> ExistsForOrderAsync(int orderId, int? excludeInvoiceId = null);
    Task AddAsync(Invoice invoice);
<<<<<<< HEAD

    /// <summary>Thêm hóa đơn; trả về false (và không ném lỗi) nếu đơn hàng đã có hóa đơn do lập đồng thời.</summary>
    Task<bool> TryAddAsync(Invoice invoice);
=======
>>>>>>> origin/main
    Task UpdateAsync(Invoice invoice);
}