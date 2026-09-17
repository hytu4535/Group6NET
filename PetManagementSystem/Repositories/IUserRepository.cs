using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetUserWithRoleAndPermissionsAsync(string username);
    Task<List<User>> GetAllWithRoleAsync();
    Task<bool> ExistsUsernameAsync(string username, int? excludeId = null);
    Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}
