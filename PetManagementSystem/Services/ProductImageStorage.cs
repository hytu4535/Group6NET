using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.Services;

public class ProductImageStorage(
    IWebHostEnvironment environment,
    ILogger<ProductImageStorage> logger) : IProductImageStorage
{
    public const string UrlPrefix = "/uploads/products/";
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private string UploadDirectory => Path.Combine(environment.WebRootPath, "uploads", "products");

    public async Task<ServiceResult<string>> SaveAsync(IFormFile file)
    {
        const string field = nameof(ViewModels.Commerce.ProductUpsertViewModel.ImageFile);

        if (file.Length == 0)
        {
            return ServiceResult<string>.Fail("File ảnh trống", field);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ServiceResult<string>.Fail("Ảnh không được vượt quá 5 MB", field);
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return ServiceResult<string>.Fail("Ảnh phải có định dạng JPG, PNG, WEBP hoặc GIF", field);
        }

        // Không tin vào đuôi file: kiểm tra chữ ký (magic bytes) để chắc chắn đây là ảnh thật.
        var header = new byte[12];
        int read;
        await using (var probe = file.OpenReadStream())
        {
            read = await probe.ReadAsync(header.AsMemory(0, header.Length));
        }

        if (!HasImageSignature(header, read))
        {
            return ServiceResult<string>.Fail("Nội dung file không phải ảnh hợp lệ", field);
        }

        Directory.CreateDirectory(UploadDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(UploadDirectory, fileName);

        await using (var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        {
            await file.CopyToAsync(target);
        }

        return ServiceResult<string>.Ok(UrlPrefix + fileName);
    }

    public bool IsManaged(string? imageUrl) =>
        !string.IsNullOrWhiteSpace(imageUrl)
        && imageUrl.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase)
        && !imageUrl.Contains("..", StringComparison.Ordinal);

    public void DeleteIfManaged(string? imageUrl)
    {
        if (!IsManaged(imageUrl))
        {
            return;
        }

        try
        {
            var physical = Path.GetFullPath(Path.Combine(UploadDirectory, Path.GetFileName(imageUrl!)));
            var root = Path.GetFullPath(UploadDirectory) + Path.DirectorySeparatorChar;
            if (physical.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(physical))
            {
                File.Delete(physical);
            }
        }
        catch (Exception ex)
        {
            // Không làm hỏng nghiệp vụ chính chỉ vì không xóa được file cũ.
            logger.LogWarning(ex, "Không xóa được file ảnh sản phẩm {ImageUrl}", imageUrl);
        }
    }

    private static bool HasImageSignature(byte[] h, int length)
    {
        if (length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF)
        {
            return true; // JPEG
        }

        if (length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47)
        {
            return true; // PNG
        }

        if (length >= 6 && h[0] == 'G' && h[1] == 'I' && h[2] == 'F' && h[3] == '8')
        {
            return true; // GIF
        }

        // WEBP: "RIFF" .... "WEBP"
        return length >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
               && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
    }
}
