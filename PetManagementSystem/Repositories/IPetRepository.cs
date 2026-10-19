using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IPetRepository
{
    Task<List<Pet>> GetAllAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetByIdAsync(int id);
    Task<List<Pet>> GetByUserIdAsync(int userId);
    Task AddAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task DeleteAsync(int id);
<<<<<<< Updated upstream
=======
    Task<PagedResult<Pet>> GetPagedByUserIdAsync(int userId, int page, int pageSize);
    Task<Pet?> GetByIdAndUserIdAsync(int petId, int userId);
    Task<List<Pet>> GetAdminPetImagePetsAsync();
    Task<PagedResult<PetImage>> GetAdminPetImagesPagedAsync(
        int? petId,
        string? searchKeyword,
        bool? isAvatar,
        int page,
        int pageSize);
    Task<PetImage?> GetAdminPetImageAsync(int petId, int imageId);
    Task<bool> HasAppointmentsAsync(int petId);
    Task AddClientPetAsync(Pet pet);
    Task UpdateClientPetAsync(Pet pet);
    Task DeleteClientPetAsync(Pet pet);
    Task AddPetImageAsync(PetImage image, bool setAsAvatar);
    Task<PetImage?> GetPetImageAsync(int imageId, int petId, int userId);
    Task DeletePetImageAsync(PetImage image);
    Task SetPetImageAsAvatarAsync(PetImage image);
>>>>>>> Stashed changes
}