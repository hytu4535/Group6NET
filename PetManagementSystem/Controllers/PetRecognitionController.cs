using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Filters;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Controllers;

[Authorize]
[NotMemberAuthorize]
[Route("PetRecognition")]
public class PetRecognitionController(
    IPetRecognitionService petRecognitionService,
    AppDbContext dbContext) : Controller
{
    private const long MaximumUploadRequestBytes =
        RecognitionImageFileAttribute.MaximumFileSizeBytes + 64 * 1024;

    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Active"] = "PetRecognition";
        return View();
    }

    [HttpGet("History")]
    public async Task<IActionResult> History(
        [FromQuery] int? petId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1)
    {
        if (petId is <= 0)
        {
            ModelState.AddModelError(
                nameof(petId),
                "Mã thú cưng phải lớn hơn 0.");
        }

        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value.Date > toDate.Value.Date)
        {
            ModelState.AddModelError(
                nameof(toDate),
                "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");
        }

        var filter = new PetRecognitionHistoryFilter
        {
            PetId = petId,
            FromDate = fromDate.HasValue
                ? DateOnly.FromDateTime(fromDate.Value)
                : null,
            ToDate = toDate.HasValue
                ? DateOnly.FromDateTime(toDate.Value)
                : null,
            Page = page,
            PageSize = 10
        };

        ViewData["Active"] = "PetRecognition";
        var model = await petRecognitionService.GetRecognitionHistoriesAsync(
            filter,
            cancellationToken);
        return View("History", model);
    }

    [HttpPost("Recognize")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaximumUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaximumUploadRequestBytes)]
    public async Task<IActionResult> Recognize(
        [FromForm] RecognizePetRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        PetRecognitionResponse aiResult;
        try
        {
            aiResult = await petRecognitionService.RecognizeAsync(
                request.File,
                cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(nameof(request.File), exception.Message);
            return ValidationProblem(ModelState);
        }

        var mappedCandidates = new List<(PetRecognitionCandidate Candidate, int DatabasePetId)>();
        foreach (var candidate in aiResult.Candidates)
        {
            if (PetIdMappingService.TryGetDatabasePetId(
                    candidate.PetId,
                    out var databasePetId))
            {
                mappedCandidates.Add((candidate, databasePetId));
            }
        }

        var petIds = mappedCandidates
            .Select(candidate => candidate.DatabasePetId)
            .Distinct()
            .ToArray();

        var pets = await dbContext.Pets
            .AsNoTracking()
            .Where(pet => petIds.Contains(pet.Id))
            .Select(pet => new PetRecognitionCandidateViewModel
            {
                DatabasePetId = pet.Id,
                PetId = $"PET{pet.Id:000}",
                PetName = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                OwnerName = pet.User == null
                    ? "Không xác định"
                    : pet.User.FullName ?? "Không xác định",
                OwnerPhone = pet.User == null ? null : pet.User.PhoneNumber,
                OwnerEmail = pet.User == null ? null : pet.User.Email,
                PetImageUrl = pet.PetImages
                    .Where(image => image.IsAvatar)
                    .OrderByDescending(image => image.Id)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
                    ?? pet.PetImages
                        .OrderByDescending(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var petsById = pets.ToDictionary(pet => pet.DatabasePetId);
        var candidates = new List<PetRecognitionCandidateViewModel>(mappedCandidates.Count);
        foreach (var (candidate, databasePetId) in mappedCandidates)
        {
            if (petsById.TryGetValue(databasePetId, out var pet))
            {
                pet.PetId = candidate.PetId;
                pet.Similarity = candidate.Similarity;
                candidates.Add(pet);
            }
        }

        ViewData["CandidateMode"] = "ai";
        ViewData["RecognitionStatus"] = aiResult.Status;
        return PartialView("_PetCandidateList", candidates);
    }

    [HttpGet("SearchOwners")]
    public async Task<IActionResult> SearchOwners(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        q = q?.Trim();
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            ViewData["CandidateMode"] = "manual";
            return PartialView("_PetCandidateList", new List<PetRecognitionCandidateViewModel>());
        }

        var results = await dbContext.Pets
            .AsNoTracking()
            .Where(pet => pet.Name.Contains(q) ||
                (pet.User != null &&
                 ((pet.User.FullName != null && pet.User.FullName.Contains(q)) ||
                  (pet.User.PhoneNumber != null && pet.User.PhoneNumber.Contains(q)) ||
                  (pet.User.Email != null && pet.User.Email.Contains(q)))))
            .OrderBy(pet => pet.Name)
            .Take(20)
            .Select(pet => new PetRecognitionCandidateViewModel
            {
                PetId = $"PET{pet.Id:000}",
                DatabasePetId = pet.Id,
                PetName = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                OwnerName = pet.User == null
                    ? "Không xác định"
                    : pet.User.FullName ?? "Không xác định",
                OwnerPhone = pet.User == null ? null : pet.User.PhoneNumber,
                OwnerEmail = pet.User == null ? null : pet.User.Email,
                PetImageUrl = pet.PetImages
                    .Where(image => image.IsAvatar)
                    .OrderByDescending(image => image.Id)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
                    ?? pet.PetImages
                        .OrderByDescending(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        ViewData["CandidateMode"] = "manual";
        return PartialView("_PetCandidateList", results);
    }

    [HttpPost("Confirm")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaximumUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaximumUploadRequestBytes)]
    public async Task<IActionResult> Confirm(
        [FromForm] ConfirmRecognitionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Không xác định được tài khoản đăng nhập.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var result = await petRecognitionService.ConfirmAsync(
            request.File,
            request.PetId,
            userId,
            request.Similarity,
            cancellationToken);

        if (result.Status != PetRecognitionConfirmationStatus.Confirmed)
        {
            return result.Status switch
            {
                PetRecognitionConfirmationStatus.InvalidImage =>
                    BadRequest(new ProblemDetails
                    {
                        Title = result.Message,
                        Status = StatusCodes.Status400BadRequest
                    }),
                PetRecognitionConfirmationStatus.StaffNotFound =>
                    BadRequest(new ProblemDetails
                    {
                        Title = result.Message,
                        Status = StatusCodes.Status400BadRequest
                    }),
                PetRecognitionConfirmationStatus.PetNotFound =>
                    NotFound(new ProblemDetails
                    {
                        Title = result.Message,
                        Status = StatusCodes.Status404NotFound
                    }),
                _ => throw new InvalidOperationException("Unknown pet recognition confirmation status.")
            };
        }

        return Ok(new
        {
            message = result.Message,
            historyId = result.HistoryId,
            petId = result.PetId,
            imageUrl = result.ImageUrl
        });
    }
}