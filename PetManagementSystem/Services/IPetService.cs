using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IPetService
{
    Task<List<Pet>> GetAllPetsAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedPetsAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetPetByIdAsync(int id);
    Task<Pet?> GetPetByTokenAsync(string token);
    Task<List<Pet>> GetPetsByUserIdAsync(int userId);
    Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet);
    Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet);
    Task<(bool Success, string ErrorMessage)> DeletePetAsync(int id);
}