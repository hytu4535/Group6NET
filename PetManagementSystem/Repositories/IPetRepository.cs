using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetByIdAsync(int id);
    Task<Pet?> GetByTokenAsync(string token);
    Task<List<Pet>> GetByUserIdAsync(int userId);
    Task AddAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task DeleteAsync(int id);
}