using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IPetService
{
    Task<List<Pet>> GetAllPetsAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedPetsAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetPetByIdAsync(int id);
    Task<List<Pet>> GetPetsByUserIdAsync(int userId);
<<<<<<< Updated upstream
    Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet);
    Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet);
=======
    Task<List<Pet>> GetAdminPetImagePetsAsync();
    Task<PagedResult<PetImage>> GetAdminPetImagesPagedAsync(
        int? petId,
        string? searchKeyword,
        bool? isAvatar,
        int page,
        int pageSize);
    Task<PetImage?> AddAdminPetImageAsync(int petId, IFormFile file, bool setAsAvatar);
    Task<(bool Success, string ErrorMessage)> SetAdminPetImageAsAvatarAsync(int petId, int imageId);
    Task<(bool Success, string ErrorMessage)> DeleteAdminPetImageAsync(int petId, int imageId);
    Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet, IFormFile? avatarFile = null);
    Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet, IFormFile? avatarFile = null);
    Task<(bool Success, string ErrorMessage)> SetPetAvatarAsync(int petId, int imageId);
>>>>>>> Stashed changes
    Task<(bool Success, string ErrorMessage)> DeletePetAsync(int id);
}