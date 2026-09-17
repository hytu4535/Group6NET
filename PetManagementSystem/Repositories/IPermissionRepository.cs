using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IPermissionRepository
{
    Task<List<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(int id);
    Task<bool> ExistsCodeAsync(string code, int? excludeId = null);
    Task AddAsync(Permission permission);
    Task UpdateAsync(Permission permission);
}
