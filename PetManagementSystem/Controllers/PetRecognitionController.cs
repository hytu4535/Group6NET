using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Controllers;

[Authorize]
[Route("PetRecognition")]
public class PetRecognitionController(
    IPetRecognitionService petRecognitionService,
    AppDbContext dbContext,
    IWebHostEnvironment environment,
    ILogger<PetRecognitionController> logger) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("Recognize")]
    public async Task<IActionResult> Recognize(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Vui lòng chọn ảnh thú cưng."
            });
        }

        try
        {
            // 1. Gửi ảnh sang AI
            var aiResult =
                await petRecognitionService.RecognizeAsync(file);

            // 2. Lấy danh sách candidate từ AI
            var candidates = new List<PetRecognitionCandidateViewModel>();

            foreach (var candidate in aiResult.Candidates)
            {
                if (!PetIdMappingService.TryGetDatabasePetId(
                        candidate.PetId,
                        out var databasePetId))
                {
                    continue;
                }

                // 3. Tìm Pet + Owner trong SQL Server
                var pet = await dbContext.Pets
                    .Include(p => p.User)
                    .Include(p => p.PetImages)
                    .FirstOrDefaultAsync(p => p.Id == databasePetId);

                if (pet is null)
                    continue;

                candidates.Add(new PetRecognitionCandidateViewModel
                {
                    PetId = candidate.PetId,
                    Similarity = candidate.Similarity,
                    PetImageUrl = pet.PetImages
                        .OrderByDescending(i => i.IsAvatar == true)
                        .ThenByDescending(i => i.UploadedAt)
                        .Select(i=> i.ImageUrl)
                        .FirstOrDefault(),

                    DatabasePetId = pet.Id,
                    PetName = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,

                    OwnerName = pet.User?.FullName ?? "Không xác định",
                    OwnerPhone = pet.User?.PhoneNumber,
                    OwnerEmail = pet.User?.Email
                });
            }

            return Ok(new
            {
                status = aiResult.Status,
                petId = aiResult.PetId,
                similarity = aiResult.Similarity,
                candidates
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Pet recognition request failed.");

            return StatusCode(500, new
            {
                message = "Không thể nhận diện thú cưng."
            });
        }
    }

    [HttpGet("SearchOwners")]
    public async Task<IActionResult> SearchOwners([FromQuery] string? q)
    {
        q = q?.Trim();
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new { candidates = Array.Empty<object>() });

        try
        {
            var pets = await dbContext.Pets
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PetImages)
                .Where(p => p.Name.Contains(q) ||
                    (p.User != null &&
                     ((p.User.FullName != null && p.User.FullName.Contains(q)) ||
                      (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(q)) ||
                      (p.User.Email != null && p.User.Email.Contains(q)))))
                .OrderBy(p => p.Name)
                .Take(20)
                .ToListAsync();

            var candidates = pets.Select(p => new
            {
                petId = $"PET{p.Id:000}",
                databasePetId = p.Id,
                petName = p.Name,
                species = p.Species,
                breed = p.Breed,
                ownerName = p.User?.FullName ?? "Không xác định",
                ownerPhone = p.User?.PhoneNumber,
                ownerEmail = p.User?.Email,
                petImageUrl = p.PetImages
                    .OrderByDescending(i => i.IsAvatar == true)
                    .ThenByDescending(i => i.UploadedAt)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
            });

            return Ok(new { candidates });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pet owner search failed.");
            return StatusCode(500, new { message = "Không thể tìm kiếm hồ sơ thú cưng." });
        }
    }

    [HttpPost("Confirm")]
    public async Task<IActionResult> Confirm(
        IFormFile file,
        int petId,
        double? similarity)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Không tìm thấy ảnh nhận diện."
            });
        }
        if (petId <= 0)
        {
            return BadRequest(new
            {
                message = "Mã thú cưng không hợp lệ."
            });
        }
        
        if (similarity.HasValue &&
            (double.IsNaN(similarity.Value) ||
             double.IsInfinity(similarity.Value) ||
             similarity.Value < 0 ||
             similarity.Value > 1))
        {
            return BadRequest(new
            {
                message = "Độ tương đồng không hợp lệ."
            });
        }

        try
        {
            // 1. Lấy User.Id từ tài khoản đang đăng nhập
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim is null ||
                !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Không xác định được tài khoản đăng nhập."
                });
            }

            // 2. Tìm Staff tương ứng với User hiện tại
            var staff = await dbContext.Staff
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff is null)
            {
                return BadRequest(new
                {
                    message = "Tài khoản hiện tại chưa được liên kết với nhân viên."
                });
            }

            // 3. Kiểm tra Pet có tồn tại không
            var pet = await dbContext.Pets
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet is null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy thú cưng."
                });
            }

            // 4. Tạo thư mục lưu ảnh
            var webRoot = environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");

            var uploadFolder = Path.Combine(
                webRoot,
                "uploads",
                "pet-recognition");

            Directory.CreateDirectory(uploadFolder);

            // 5. Tạo tên file mới để tránh trùng
            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                extension = ".jpg";
            }

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath =
                Path.Combine(uploadFolder, fileName);

            // 6. Lưu ảnh
            await using (var stream = new FileStream(
                filePath,
                FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            // URL để lưu vào database
            var imageUrl =
                $"/uploads/pet-recognition/{fileName}";

            // 7. Tạo lịch sử nhận diện
            var history = new PetRecognitionHistory
            {
                StaffId = staff.Id,
                ImageUrl = imageUrl,
                SelectedPetId = pet.Id,
                DetectedSpecies = pet.Species,
                DetectedBreed = pet.Breed,
                TopSimilarity = similarity.HasValue ? Convert.ToDecimal(similarity.Value) : null,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.PetRecognitionHistories.Add(history);

            // 8. Lưu database
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = similarity.HasValue
                    ? "Đã xác nhận thú cưng và lưu lịch sử nhận diện AI."
                    : "Đã xác nhận hồ sơ thủ công và lưu lịch sử.",
                historyId = history.Id,
                petId = pet.Id,
                imageUrl
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Confirm pet recognition failed.");

            return StatusCode(500, new
            {
                message = "Không thể lưu xác nhận thú cưng."
            });
        }
    }
}