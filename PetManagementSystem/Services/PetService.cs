using System.Security.Claims;
using PetManagementSystem.ViewModels;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class PetService(
    IPetRepository petRepository,
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment,
    ILogger<PetService> logger) : IPetService
{
    private const string AvatarUploadRelativePath = "uploads/pets/avatars";
    private const string AvatarUploadUrlPrefix = "/uploads/pets/avatars/";
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

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

    public async Task<(bool Success, string ErrorMessage)> CreatePetAsync(
        Pet pet,
        IFormFile? avatarFile = null)
    {
        string? savedAvatarPath = null;
        try
        {
            if (IsUserAdmin())
            {
                if (pet.UserId <= 0)
                {
                    return (false, "Vui lòng chọn chủ nuôi cho thú cưng.");
                }
            }
            else
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

            (string Url, string FilePath)? avatar = avatarFile is null
                ? null
                : await SaveAvatarAsync(avatarFile);
            savedAvatarPath = avatar?.FilePath;

            await petRepository.AddAsync(pet, avatar?.Url);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            DeleteFileIfExists(savedAvatarPath);
            logger.LogError(ex, "Lỗi hệ thống khi thêm thú cưng mới.");
            return (false, ex is InvalidDataException
                ? ex.Message
                : "Không thể thêm thú cưng do lỗi hệ thống.");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> UpdatePetAsync(
        Pet pet,
        IFormFile? avatarFile = null)
    {
        string? savedAvatarPath = null;
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

            (string Url, string FilePath)? avatar = avatarFile is null
                ? null
                : await SaveAvatarAsync(avatarFile);
            savedAvatarPath = avatar?.FilePath;

            await petRepository.UpdateAsync(existingPet, avatar?.Url);

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            DeleteFileIfExists(savedAvatarPath);
            logger.LogError(ex, "Lỗi hệ thống khi cập nhật thú cưng ID: {PetId}", pet.Id);
            return (false, ex is InvalidDataException
                ? ex.Message
                : "Không thể cập nhật thú cưng do lỗi hệ thống.");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> SetPetAvatarAsync(int petId, int imageId)
    {
        try
        {
            var pet = await petRepository.GetByIdAsync(petId);
            if (pet is null)
            {
                return (false, "Không tìm thấy thú cưng.");
            }

            var currentUserId = GetCurrentUserId();
            if (!IsUserAdmin() && pet.UserId != currentUserId)
            {
                return (false, "Bạn không có quyền thay đổi ảnh đại diện của thú cưng này.");
            }

            var updated = await petRepository.SetAvatarAsync(petId, imageId);
            return updated
                ? (true, string.Empty)
                : (false, "Ảnh được chọn không thuộc thư viện của thú cưng này.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi đặt ảnh {ImageId} làm ảnh đại diện cho thú cưng {PetId}.", imageId, petId);
            return (false, "Không thể cập nhật ảnh đại diện do lỗi hệ thống.");
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

    private async Task<(string Url, string FilePath)> SaveAvatarAsync(IFormFile file)
    {
        var validation = new RecognitionImageFileAttribute();
        if (!validation.IsValid(file))
        {
            throw new InvalidDataException(validation.FormatErrorMessage(nameof(file)));
        }

        if (!await HasValidImageSignatureAsync(file))
        {
            throw new InvalidDataException("Nội dung file không phải ảnh JPG, JPEG, PNG hoặc WEBP hợp lệ.");
        }

        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadDirectory = Path.Combine(
            webRoot,
            AvatarUploadRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(uploadDirectory, fileName);
        try
        {
            await using var destination = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);
            await using var source = file.OpenReadStream();
            await source.CopyToAsync(destination);
        }
        catch
        {
            DeleteFileIfExists(filePath);
            throw;
        }

        return ($"{AvatarUploadUrlPrefix}{fileName}", filePath);
    }

    private static async Task<bool> HasValidImageSignatureAsync(IFormFile file)
    {
        var buffer = new byte[12];
        var bytesRead = 0;
        await using var stream = file.OpenReadStream();
        while (bytesRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(bytesRead));
            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        return Path.GetExtension(file.FileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 &&
                                 buffer[0] == 0xFF &&
                                 buffer[1] == 0xD8 &&
                                 buffer[2] == 0xFF,
            ".png" => bytesRead >= PngSignature.Length &&
                      buffer.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature),
            ".webp" => bytesRead >= 12 &&
                       buffer.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                       buffer.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private void DeleteFileIfExists(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Không thể xóa file ảnh thú cưng cũ: {FilePath}", path);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "Không đủ quyền xóa file ảnh thú cưng cũ: {FilePath}", path);
        }
    }
}