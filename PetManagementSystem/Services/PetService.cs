using System.Security.Claims;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using Microsoft.AspNetCore.Hosting;

namespace PetManagementSystem.Services;

public class PetService(
    IPetRepository petRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PetService> logger,
    IWebHostEnvironment webHostEnvironment) : IPetService
{
    private const long MaxPetImageSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedPetImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

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

    public Task<PagedResult<Pet>> GetClientPetsPagedAsync(int userId, int page, int pageSize)
    {
        return petRepository.GetPagedByUserIdAsync(userId, page, pageSize);
    }

    public Task<Pet?> GetClientPetAsync(int userId, int petId)
    {
        return petRepository.GetByIdAndUserIdAsync(petId, userId);
    }

    public async Task<Pet> CreateClientPetAsync(int userId, Pet pet)
    {
        pet.UserId = userId;
        pet.Name = pet.Name.Trim();
        pet.Species = pet.Species.Trim();
        pet.Breed = pet.Breed?.Trim();
        await petRepository.AddClientPetAsync(pet);
        return pet;
    }

    public async Task<Pet?> UpdateClientPetAsync(int userId, int petId, Pet updatedPet)
    {
        var pet = await petRepository.GetByIdAndUserIdAsync(petId, userId);
        if (pet is null)
        {
            return null;
        }

        pet.Name = updatedPet.Name.Trim();
        pet.Species = updatedPet.Species.Trim();
        pet.Breed = updatedPet.Breed?.Trim();
        pet.Gender = updatedPet.Gender;
        pet.WeightKg = updatedPet.WeightKg;
        await petRepository.UpdateClientPetAsync(pet);
        return pet;
    }

    public async Task<bool> DeleteClientPetAsync(int userId, int petId)
    {
        var pet = await petRepository.GetByIdAndUserIdAsync(petId, userId);
        if (pet is null)
        {
            return false;
        }

        await petRepository.DeleteClientPetAsync(pet);
        var petDirectory = GetPetImageDirectory(userId, petId);
        if (Directory.Exists(petDirectory))
        {
            Directory.Delete(petDirectory, recursive: true);
        }
        return true;
    }

    public async Task<PetImage?> AddClientPetImageAsync(
        int userId,
        int petId,
        IFormFile file,
        bool setAsAvatar)
    {
        await ValidateClientPetImageAsync(file);
        var extension = Path.GetExtension(file.FileName);

        if (await petRepository.GetByIdAndUserIdAsync(petId, userId) is null)
        {
            return null;
        }

        var directory = GetPetImageDirectory(userId, petId);
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(directory, fileName);
        var image = new PetImage
        {
            PetId = petId,
            ImageUrl = $"/uploads/pets/{userId}/{petId}/{fileName}",
            CreatedAt = DateTime.UtcNow
        };

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            await petRepository.AddPetImageAsync(image, setAsAvatar);
            return image;
        }
        catch
        {
            File.Delete(physicalPath);
            throw;
        }
    }

    public async Task ValidateClientPetImageAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxPetImageSize)
        {
            throw new ArgumentException("Ảnh phải có dung lượng từ 1 byte đến 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedPetImageExtensions.Contains(extension))
        {
            throw new ArgumentException("Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.");
        }

        var signature = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(signature);
        var isJpeg = bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF;
        var isPng = bytesRead >= 8 &&
            signature.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = bytesRead >= 12 &&
            signature.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            signature.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        var extensionMatchesContent =
            (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
             extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) && isJpeg ||
            extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && isPng ||
            extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && isWebp;

        if (!extensionMatchesContent)
        {
            throw new ArgumentException("Nội dung tệp không khớp với định dạng ảnh được hỗ trợ.");
        }
    }

    public async Task<List<PetImage>?> GetClientPetImagesAsync(int userId, int petId)
    {
        var pet = await petRepository.GetByIdAndUserIdAsync(petId, userId);
        return pet?.PetImages.OrderByDescending(image => image.CreatedAt).ToList();
    }

    public async Task<bool> DeleteClientPetImageAsync(int userId, int petId, int imageId)
    {
        var image = await petRepository.GetPetImageAsync(imageId, petId, userId);
        if (image is null)
        {
            return false;
        }

        await petRepository.DeletePetImageAsync(image);
        DeletePetImageFile(image.ImageUrl);
        return true;
    }

    public async Task<bool> SetClientPetImageAsAvatarAsync(int userId, int petId, int imageId)
    {
        var image = await petRepository.GetPetImageAsync(imageId, petId, userId);
        if (image is null)
        {
            return false;
        }

        await petRepository.SetPetImageAsAvatarAsync(image);
        return true;
    }

    private string GetPetImageDirectory(int userId, int petId)
    {
        return Path.Combine(webHostEnvironment.WebRootPath, "uploads", "pets", userId.ToString(), petId.ToString());
    }

    private void DeletePetImageFile(string imageUrl)
    {
        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.GetFullPath(Path.Combine(webHostEnvironment.WebRootPath, relativePath));
        var uploadRoot = Path.GetFullPath(Path.Combine(webHostEnvironment.WebRootPath, "uploads", "pets"))
            + Path.DirectorySeparatorChar;
        if (physicalPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}