using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface ISupplierRepository
{
    Task<PetManagementSystem.Models.PagedResult<Supplier>> SearchAsync(string? search, int page, int pageSize);
    Task<List<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(int id);
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
    Task<bool> HasReceiptsAsync(int id);
    Task<Dictionary<int, int>> GetReceiptCountsAsync(IEnumerable<int> supplierIds);
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task DeleteAsync(Supplier supplier);
}
