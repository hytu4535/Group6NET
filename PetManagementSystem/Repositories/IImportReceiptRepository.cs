using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IImportReceiptRepository
{
    Task<PetManagementSystem.Models.PagedResult<ImportReceipt>> SearchAsync(string? search, int? supplierId, DateTime? from, DateTime? to, int page, int pageSize);
    Task<PetManagementSystem.Models.PagedResult<ImportDetail>> SearchDetailsAsync(int? receiptId, string? search, int page, int pageSize);

    /// <summary>Phiếu nhập kèm nhà cung cấp, nhân viên và chi tiết (có theo dõi thay đổi).</summary>
    Task<ImportReceipt?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// Các repository dùng chung một <c>AppDbContext</c> (scoped) nên thay đổi tồn kho của
    /// sản phẩm đã được nạp trước đó sẽ được lưu cùng transaction với phiếu nhập.
    /// </summary>
    Task AddAsync(ImportReceipt receipt);
    Task UpdateAsync(ImportReceipt receipt);
    Task DeleteAsync(ImportReceipt receipt);
}