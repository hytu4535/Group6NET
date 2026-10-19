using System.Security.Claims;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class PetService(
    IPetRepository petRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PetService> logger) : IPetService
{
    public async Task<List<Pet>> GetAllPetsAsync(string? searchKeyword = null)
    {
        try
        {
            return await petRepository.GetAllAsync(searchKeyword);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách thú cưng với từ khóa: {Keyword}", searchKeyword);
            return [];
        }
    }

    public async Task<PagedResult<Pet>> GetPagedPetsAsync(string? searchKeyword, int page, int pageSize)
    {
        try
        {
            return await petRepository.GetPagedAsync(searchKeyword, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy trang danh sách thú cưng.");
            return new PagedResult<Pet>([], page, pageSize, 0);
        }
    }

    public async Task<Pet?> GetPetByIdAsync(int id)
    {
        try
        {
            return await petRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy thông tin thú cưng ID: {PetId}", id);
            return null;
        }
    }

    public async Task<List<Pet>> GetPetsByUserIdAsync(int userId)
    {
        try
        {
            return await petRepository.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy danh sách thú cưng của User ID: {UserId}", userId);
            return [];
        }
    }

<<<<<<< Updated upstream
    public async Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet)
=======
    public Task<List<Pet>> GetAdminPetImagePetsAsync()
    {
        return petRepository.GetAdminPetImagePetsAsync();
    }

    public Task<PagedResult<PetImage>> GetAdminPetImagesPagedAsync(
        int? petId,
        string? searchKeyword,
        bool? isAvatar,
        int page,
        int pageSize)
    {
        return petRepository.GetAdminPetImagesPagedAsync(petId, searchKeyword, isAvatar, page, pageSize);
    }

    public async Task<PetImage?> AddAdminPetImageAsync(int petId, IFormFile file, bool setAsAvatar)
    {
        await ValidateClientPetImageAsync(file);
        var pet = await petRepository.GetByIdAsync(petId);
        if (pet is null)
        {
            return null;
        }

        var (imageUrl, filePath) = await SaveAvatarAsync(file);
        var image = new PetImage
        {
            PetId = petId,
            ImageUrl = imageUrl,
            UploadedAt = DateTime.UtcNow,
            Pet = pet
        };

        try
        {
            await petRepository.AddPetImageAsync(image, setAsAvatar);
            return image;
        }
        catch
        {
            DeleteFileIfExists(filePath);
            throw;
        }
    }

    public async Task<(bool Success, string ErrorMessage)> SetAdminPetImageAsAvatarAsync(
        int petId,
        int imageId)
    {
        var updated = await petRepository.SetAvatarAsync(petId, imageId);
        return updated
            ? (true, string.Empty)
            : (false, "Không tìm thấy ảnh thuộc thú cưng đã chọn.");
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteAdminPetImageAsync(int petId, int imageId)
    {
        var image = await petRepository.GetAdminPetImageAsync(petId, imageId);
        if (image is null)
        {
            return (false, "Không tìm thấy ảnh thú cưng.");
        }

        await petRepository.DeletePetImageAsync(image);
        DeletePetImageFile(image.ImageUrl);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> CreatePetAsync(
        Pet pet,
        IFormFile? avatarFile = null)
>>>>>>> Stashed changes
    {
        try
        {
            // Tự động gán UserId từ Session/Cookie của người dùng đang đăng nhập nếu chưa truyền
            if (pet.UserId <= 0)
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return (false, "Người dùng chưa đăng nhập hoặc không xác định được danh tính.");
                }
                pet.UserId = currentUserId.Value;
            }

            if (string.IsNullOrWhiteSpace(pet.Name))
            {
                return (false, "Tên thú cưng không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(pet.Species))
            {
                return (false, "Loài thú cưng không được để trống.");
            }

            await petRepository.AddAsync(pet);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi thêm thú cưng mới.");
            return (false, "Không thể thêm thú cưng do lỗi hệ thống.");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> UpdatePetAsync(Pet pet)
    {
        try
        {
            var existingPet = await petRepository.GetByIdAsync(pet.Id);
            if (existingPet is null)
            {
                return (false, "Không tìm thấy thông tin thú cưng.");
            }

            // Kiểm tra quyền chỉnh sửa
            var currentUserId = GetCurrentUserId();
            var isUserAdmin = IsUserAdmin();
            if (!isUserAdmin && existingPet.UserId != currentUserId)
            {
                return (false, "Bạn không có quyền chỉnh sửa thú cưng này.");
            }

            if (isUserAdmin)
            {
                if (pet.UserId <= 0)
                {
                    return (false, "Vui lòng chọn chủ nuôi cho thú cưng.");
                }

                existingPet.UserId = pet.UserId;
            }

            existingPet.Name = pet.Name.Trim();
            existingPet.Species = pet.Species.Trim();
            existingPet.Breed = pet.Breed?.Trim();
            existingPet.Gender = pet.Gender;
            existingPet.WeightKg = pet.WeightKg;

            await petRepository.UpdateAsync(existingPet);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi cập nhật thú cưng ID: {PetId}", pet.Id);
            return (false, "Không thể cập nhật thú cưng do lỗi hệ thống.");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> DeletePetAsync(int id)
    {
        try
        {
            var pet = await petRepository.GetByIdAsync(id);
            if (pet is null)
            {
                return (false, "Không tìm thấy thú cưng cần xóa.");
            }

            // Kiểm tra quyền xóa
            var currentUserId = GetCurrentUserId();
            var isUserAdmin = IsUserAdmin();
            if (!isUserAdmin && pet.UserId != currentUserId)
            {
                return (false, "Bạn không có quyền xóa thú cưng này.");
            }

            await petRepository.DeleteAsync(id);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi xóa thú cưng ID: {PetId}", id);
            return (false, "Không thể xóa thú cưng do lỗi hệ thống.");
        }
    }

    private int? GetCurrentUserId()
    {
        var claimValue = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }

    private bool IsUserAdmin()
    {
        return httpContextAccessor.HttpContext?.User?.IsInRole("admin") ?? false;
    }
}