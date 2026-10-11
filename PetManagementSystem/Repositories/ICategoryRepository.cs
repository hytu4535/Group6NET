using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface ICategoryRepository
{
    Task<PetManagementSystem.Models.PagedResult<Category>> SearchAsync(string? search, int page, int pageSize);
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> ExistsNameAsync(string name, int? excludeId = null);
    Task<bool> HasProductsAsync(int id);
    Task<Dictionary<int, int>> GetProductCountsAsync(IEnumerable<int> categoryIds);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}