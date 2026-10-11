using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetByIdAsync(int id);
    Task<List<Pet>> GetByUserIdAsync(int userId);
    Task AddAsync(Pet pet, string? avatarUrl = null);
    Task UpdateAsync(Pet pet, string? avatarUrl = null);
    Task<bool> SetAvatarAsync(int petId, int imageId);
    Task DeleteAsync(int id);
}