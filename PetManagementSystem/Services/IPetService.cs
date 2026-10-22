using Microsoft.AspNetCore.Http;
using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IPetService
{
    Task<List<Pet>> GetAllPetsAsync(string? searchKeyword = null);
    Task<PagedResult<Pet>> GetPagedPetsAsync(string? searchKeyword, int page, int pageSize);
    Task<Pet?> GetPetByIdAsync(int id);
    Task<Pet?> GetPetByTokenAsync(string token);
    Task<List<Pet>> GetPetsByUserIdAsync(int userId);
    Task<List<Pet>> GetAdminPetImagePetsAsync();
    Task<PagedResult<PetImage>> GetAdminPetImagesPagedAsync(int? petId, string? searchKeyword, bool? isAvatar, int page, int pageSize);
    Task<PetImage?> AddAdminPetImageAsync(int petId, IFormFile file, bool setAsAvatar);
    Task<(bool Success, string ErrorMessage)> SetAdminPetImageAsAvatarAsync(int petId, int imageId);
    Task<(bool Success, string ErrorMessage)> DeleteAdminPetImageAsync(int petId, int imageId);
    Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet, IFormFile? avatarFile = null);
    Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet, IFormFile? avatarFile = null);
    Task<(bool Success, string ErrorMessage)> SetPetAvatarAsync(int petId, int imageId);
    Task<(bool Success, string ErrorMessage)> DeletePetAsync(int id);
    Task<PagedResult<Pet>> GetClientPetsPagedAsync(int userId, int page, int pageSize);
    Task<Pet?> GetClientPetAsync(int userId, int petId);
    Task<Pet> CreateClientPetAsync(int userId, Pet pet);
    Task<Pet?> UpdateClientPetAsync(int userId, int petId, Pet pet);
    Task<(bool Success, string? ErrorMessage)> DeleteClientPetAsync(int userId, int petId);
    Task ValidateClientPetImageAsync(IFormFile file);
    Task<PetImage?> AddClientPetImageAsync(int userId, int petId, IFormFile file, bool setAsAvatar);
    Task<List<PetImage>?> GetClientPetImagesAsync(int userId, int petId);
    Task<bool> DeleteClientPetImageAsync(int userId, int petId, int imageId);
    Task<bool> SetClientPetImageAsAvatarAsync(int userId, int petId, int imageId);
}