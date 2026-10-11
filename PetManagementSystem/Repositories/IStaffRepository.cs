using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IStaffRepository
{
    Task<List<Staff>> GetAllWithUserAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<bool> ExistsByUserIdAsync(int userId, int? excludeId = null);
    Task AddAsync(Staff staff);
    Task UpdateAsync(Staff staff);
}
