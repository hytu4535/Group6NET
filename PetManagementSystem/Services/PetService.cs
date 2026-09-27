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

    public async Task<(bool Success, string ErrorMessage)> CreatePetAsync(Pet pet)
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