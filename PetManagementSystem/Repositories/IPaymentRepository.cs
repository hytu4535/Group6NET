using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IPaymentRepository
{
    Task<PetManagementSystem.Models.PagedResult<Payment>> SearchAsync(string? search, string? status, string? method, int page, int pageSize);
    Task<Payment?> GetByIdAsync(int id);
    Task<bool> ExistsTransactionCodeAsync(string code, int? excludeId = null);

    /// <summary>Tổng tiền các giao dịch đã thanh toán thành công của một đơn hàng.</summary>
    Task<decimal> GetCompletedTotalAsync(int orderId, int? excludePaymentId = null);

    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(Payment payment);
}