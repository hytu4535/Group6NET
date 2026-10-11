using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetRoleWithPermissionsAsync(int roleId);
    Task<Role?> GetByNameAsync(string roleName);
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int roleId);
    Task<bool> ExistsNameAsync(string roleName, int? excludeId = null);
    Task AddAsync(Role role, IEnumerable<int> permissionIds);
    Task UpdateAsync(Role role, IEnumerable<int> permissionIds);
}
