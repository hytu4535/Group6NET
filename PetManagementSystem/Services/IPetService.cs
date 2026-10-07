using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IPetService
{
    Task<List<Pet>> GetAllPetsAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedPetsAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetPetByIdAsync(int id);
    Task<List<Pet>> GetPetsByUserIdAsync(int userId);
    Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet);
    Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet);
    Task<(bool Success, string ErrorMessage)> DeletePetAsync(int id);
    Task<PagedResult<Pet>> GetClientPetsPagedAsync(int userId, int page, int pageSize);
    Task<Pet?> GetClientPetAsync(int userId, int petId);
    Task<Pet> CreateClientPetAsync(int userId, Pet pet);
    Task<Pet?> UpdateClientPetAsync(int userId, int petId, Pet pet);
    Task<bool> DeleteClientPetAsync(int userId, int petId);
    Task ValidateClientPetImageAsync(IFormFile file);
    Task<PetImage?> AddClientPetImageAsync(int userId, int petId, IFormFile file, bool setAsAvatar);
    Task<List<PetImage>?> GetClientPetImagesAsync(int userId, int petId);
    Task<bool> DeleteClientPetImageAsync(int userId, int petId, int imageId);
    Task<bool> SetClientPetImageAsAvatarAsync(int userId, int petId, int imageId);
}