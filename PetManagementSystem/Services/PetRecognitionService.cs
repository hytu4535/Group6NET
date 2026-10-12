using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public class PetRecognitionService(
    HttpClient httpClient,
    AppDbContext dbContext,
    IWebHostEnvironment environment) : IPetRecognitionService
{
    private const string RecognizeEndpoint = "/api/ai/recognize";
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public async Task<PetRecognitionResponse> RecognizeAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!IsFileMetadataValid(file) || !await HasValidImageSignatureAsync(file, cancellationToken))
        {
            throw new InvalidDataException("Ảnh không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG hoặc WEBP tối đa 5 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(extension));
        content.Add(fileContent, "file", $"recognition{extension}");

        using var response = await httpClient.PostAsync(
            RecognizeEndpoint,
            content,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PetRecognitionResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("AI service returned an empty response.");
    }

    public async Task<PetRecognitionConfirmationResult> ConfirmAsync(
        IFormFile file,
        int petId,
        int userId,
        double? similarity,
        CancellationToken cancellationToken)
    {
        if (!IsFileMetadataValid(file) || !await HasValidImageSignatureAsync(file, cancellationToken))
        {
            return new PetRecognitionConfirmationResult(
                PetRecognitionConfirmationStatus.InvalidImage,
                "Ảnh không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG hoặc WEBP tối đa 5 MB.");
        }

        var staffId = await dbContext.Staff
            .AsNoTracking()
            .Where(staff => staff.UserId == userId)
            .Select(staff => (int?)staff.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (staffId is null)
        {
            return new PetRecognitionConfirmationResult(
                PetRecognitionConfirmationStatus.StaffNotFound,
                "Tài khoản hiện tại chưa được liên kết với nhân viên.");
        }

        var pet = await dbContext.Pets
            .AsNoTracking()
            .Where(candidate => candidate.Id == petId)
            .Select(candidate => new
            {
                candidate.Id,
                candidate.Species,
                candidate.Breed
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (pet is null)
        {
            return new PetRecognitionConfirmationResult(
                PetRecognitionConfirmationStatus.PetNotFound,
                "Không tìm thấy thú cưng.");
        }

        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var uploadFolder = Path.Combine(webRoot, "uploads", "pet-recognition");
        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);
        var imageUrl = $"/uploads/pet-recognition/{fileName}";

        var imageCreated = false;
        try
        {
            await using (var destination = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true))
            {
                imageCreated = true;
                await using var source = file.OpenReadStream();
                await source.CopyToAsync(destination, cancellationToken);
            }

            var history = new PetRecognitionHistory
            {
                StaffId = staffId.Value,
                ImageUrl = imageUrl,
                SelectedPetId = pet.Id,
                DetectedSpecies = pet.Species,
                DetectedBreed = pet.Breed,
                TopSimilarity = similarity.HasValue ? Convert.ToDecimal(similarity.Value) : null,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.PetRecognitionHistories.Add(history);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new PetRecognitionConfirmationResult(
                PetRecognitionConfirmationStatus.Confirmed,
                similarity.HasValue
                    ? "Đã xác nhận thú cưng và lưu lịch sử nhận diện AI."
                    : "Đã xác nhận hồ sơ thủ công và lưu lịch sử.",
                history.Id,
                pet.Id,
                imageUrl);
        }
        catch
        {
            if (imageCreated && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            throw;
        }
    }

    public async Task<PetRecognitionHistoryPageViewModel> GetRecognitionHistoriesAsync(
        PetRecognitionHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var fromDate = filter.FromDate?.ToDateTime(TimeOnly.MinValue);
        DateTime? toDateExclusive = filter.ToDate is { } toDate && toDate < DateOnly.MaxValue
            ? toDate.AddDays(1).ToDateTime(TimeOnly.MinValue)
            : null;

        var query = dbContext.PetRecognitionHistories
            .AsNoTracking()
            .AsQueryable();

        if (filter.PetId.HasValue)
        {
            query = query.Where(history => history.SelectedPetId == filter.PetId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(history => history.CreatedAt >= fromDate.Value);
        }

        if (toDateExclusive.HasValue)
        {
            query = query.Where(history => history.CreatedAt < toDateExclusive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Min(page, Math.Max(totalPages, 1));

        var items = await query
            .OrderByDescending(history => history.CreatedAt)
            .ThenByDescending(history => history.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(history => new PetRecognitionHistoryItemViewModel
            {
                Id = history.Id,
                PetId = history.SelectedPetId,
                PetName = history.SelectedPet == null
                    ? "Chưa xác định"
                    : history.SelectedPet.Name,
                PetSpecies = history.DetectedSpecies ??
                    (history.SelectedPet == null ? null : history.SelectedPet.Species),
                PetBreed = history.DetectedBreed ??
                    (history.SelectedPet == null ? null : history.SelectedPet.Breed),
                OwnerName = history.SelectedPet == null || history.SelectedPet.User == null
                    ? "Không xác định"
                    : history.SelectedPet.User.FullName ?? history.SelectedPet.User.Username,
                OwnerPhone = history.SelectedPet == null || history.SelectedPet.User == null
                    ? null
                    : history.SelectedPet.User.PhoneNumber,
                InputImageUrl = history.ImageUrl,
                PetImageUrl = history.SelectedPet == null
                    ? null
                    : history.SelectedPet.PetImages
                        .Where(image => image.IsAvatar)
                        .OrderByDescending(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault()
                        ?? history.SelectedPet.PetImages
                            .OrderByDescending(image => image.Id)
                            .Select(image => image.ImageUrl)
                            .FirstOrDefault(),
                SimilarityScore = history.TopSimilarity,
                RecognizedAt = history.CreatedAt,
                StaffName = history.Staff == null || history.Staff.User == null
                    ? "Không xác định"
                    : history.Staff.User.FullName ?? history.Staff.User.Username,
                Notes = history.DetectedSpecies == null && history.DetectedBreed == null
                    ? null
                    : (history.DetectedSpecies ?? string.Empty) +
                      (history.DetectedBreed == null
                          ? string.Empty
                          : (history.DetectedSpecies == null ? string.Empty : " · ") + history.DetectedBreed)
            })
            .ToListAsync(cancellationToken);

        var pets = await dbContext.Pets
            .AsNoTracking()
            .OrderBy(pet => pet.Name)
            .Select(pet => new PetRecognitionPetOptionViewModel
            {
                Id = pet.Id,
                Name = pet.Name
            })
            .ToListAsync(cancellationToken);

        var normalizedFilter = new PetRecognitionHistoryFilter
        {
            Page = page,
            PageSize = pageSize,
            PetId = filter.PetId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate
        };

        return new PetRecognitionHistoryPageViewModel
        {
            Filter = normalizedFilter,
            Items = items,
            Pets = pets,
            TotalCount = totalCount
        };
    }

    private static bool IsFileMetadataValid(IFormFile file) =>
        new RecognitionImageFileAttribute().IsValid(file);

    private static async Task<bool> HasValidImageSignatureAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[12];
        var bytesRead = 0;

        await using var stream = file.OpenReadStream();
        while (bytesRead < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(bytesRead, buffer.Length - bytesRead),
                cancellationToken);
            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        var extension = Path.GetExtension(file.FileName);
        return extension.ToLowerInvariant() switch
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

    private static string GetContentType(string extension) =>
        extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidDataException("Unsupported image extension.")
        };
}