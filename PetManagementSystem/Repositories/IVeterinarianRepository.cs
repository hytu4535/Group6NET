using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IVeterinarianRepository
{
    Task<List<Veterinarian>> GetAllWithUserAsync();
    Task<Veterinarian?> GetByIdAsync(int id);
    Task<bool> ExistsByUserIdAsync(int userId, int? excludeId = null);
    Task AddAsync(Veterinarian veterinarian);
    Task UpdateAsync(Veterinarian veterinarian);
}
