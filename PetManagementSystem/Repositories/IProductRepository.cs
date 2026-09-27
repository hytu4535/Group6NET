using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IProductRepository
{
    Task<PagedResult<Product>> SearchAsync(string? search, int? categoryId, string? status, int page, int pageSize);

    /// <summary>Danh sách rút gọn (Id, tên, đơn vị, giá, tồn kho) dùng cho ô chọn sản phẩm.</summary>
    Task<List<Product>> GetOptionsAsync(bool onlyActive);

    Task<Product?> GetByIdAsync(int id);

    /// <summary>Lấy các sản phẩm (có theo dõi thay đổi) để cập nhật tồn kho.</summary>
    Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);

    Task<bool> ExistsNameAsync(string name, int categoryId, int? excludeId = null);

    /// <summary>Sản phẩm đã xuất hiện trong phiếu nhập, giỏ hàng hoặc đơn hàng.</summary>
    Task<bool> IsReferencedAsync(int id);

    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
