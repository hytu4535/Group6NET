using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Pets;

namespace PetManagementSystem.Controllers.Api;

[Authorize]
[Route("api/admin/pet-images")]
public class AdminPetImagesApiController(
    IPetService petService,
    ILogger<AdminPetImagesApiController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "pet_images.view")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminPetImageListItemViewModel>>>> GetPetImages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] int? petId = null,
        [FromQuery] string? searchKeyword = null,
        [FromQuery] bool? isAvatar = null)
    {
        if (page < 1 || page > 1_000_000 || pageSize < 1 || pageSize > 100 || petId <= 0)
        {
            return BadRequest(ApiResponse<IReadOnlyList<AdminPetImageListItemViewModel>>.Fail(
                "Tham số phân trang hoặc bộ lọc không hợp lệ."));
        }
        if (searchKeyword?.Length > 100)
        {
            return BadRequest(ApiResponse<IReadOnlyList<AdminPetImageListItemViewModel>>.Fail(
                "Từ khóa tìm kiếm không được vượt quá 100 ký tự."));
        }

        var result = await petService.GetAdminPetImagesPagedAsync(
            petId, searchKeyword, isAvatar, page, pageSize);
        var data = result.Items.Select(ToListItem).ToList();
        return Ok(ApiResponse<IReadOnlyList<AdminPetImageListItemViewModel>>.Ok(
            data,
            "Lấy danh sách ảnh thú cưng thành công.",
            result.Page,
            result.PageSize,
            result.TotalItems));
    }

    [HttpGet("pets")]
    [Authorize(Policy = "pet_images.view")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminPetImagePetOptionViewModel>>>> GetPets()
    {
        var pets = await petService.GetAdminPetImagePetsAsync();
        var data = pets.Select(pet => new AdminPetImagePetOptionViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            OwnerName = pet.User?.FullName ?? pet.User?.Username ?? $"User #{pet.UserId}"
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<AdminPetImagePetOptionViewModel>>.Ok(
            data, "Lấy danh sách thú cưng thành công."));
    }

    [HttpPost]
    [Authorize(Policy = "pet_images.create")]
    [Consumes("multipart/form-data")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<AdminPetImageListItemViewModel>>> UploadPetImage(
        [FromForm] AdminPetImageUploadViewModel model)
    {
        if (!ModelState.IsValid || model.Image is null)
        {
            return BadRequest(ApiResponse<AdminPetImageListItemViewModel>.Fail(
                "Dữ liệu ảnh không hợp lệ.", [.. GetValidationErrors()]));
        }

        try
        {
            var image = await petService.AddAdminPetImageAsync(model.PetId, model.Image, model.SetAsAvatar);
            if (image is null)
            {
                return NotFound(ApiResponse<AdminPetImageListItemViewModel>.Fail(
                    "Không tìm thấy thú cưng đã chọn."));
            }

            return Ok(ApiResponse<AdminPetImageListItemViewModel>.Ok(
                ToListItem(image),
                "Tải ảnh thú cưng lên thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<AdminPetImageListItemViewModel>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi tải ảnh lên cho thú cưng {PetId}.", model.PetId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminPetImageListItemViewModel>.Fail(
                    "Không thể tải ảnh lên do lỗi hệ thống.", "Vui lòng thử lại sau."));
        }
    }

    [HttpPut("{petId:int}/{imageId:int}/avatar")]
    [Authorize(Policy = "pet_images.update")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> SetAvatar(int petId, int imageId)
    {
        try
        {
            var result = await petService.SetAdminPetImageAsAvatarAsync(petId, imageId);
            return result.Success
                ? Ok(ApiResponse<object>.Ok(new { id = imageId }, "Đã cập nhật ảnh đại diện."))
                : NotFound(ApiResponse<object>.Fail(result.ErrorMessage));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi đặt ảnh {ImageId} làm ảnh đại diện cho thú cưng {PetId}.", imageId, petId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Không thể cập nhật ảnh đại diện do lỗi hệ thống."));
        }
    }

    [HttpDelete("{petId:int}/{imageId:int}")]
    [Authorize(Policy = "pet_images.delete")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> DeletePetImage(int petId, int imageId)
    {
        try
        {
            var result = await petService.DeleteAdminPetImageAsync(petId, imageId);
            return result.Success
                ? Ok(ApiResponse<object>.Ok(new { id = imageId }, "Đã xóa ảnh thú cưng."))
                : NotFound(ApiResponse<object>.Fail(result.ErrorMessage));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi xóa ảnh {ImageId} của thú cưng {PetId}.", imageId, petId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("Không thể xóa ảnh do lỗi hệ thống."));
        }
    }

    private static AdminPetImageListItemViewModel ToListItem(PetImage image)
    {
        return ToListItem(
            image,
            image.Pet?.Name ?? string.Empty,
            image.Pet?.User?.FullName ?? image.Pet?.User?.Username
                ?? (image.Pet is null ? null : $"User #{image.Pet.UserId}"));
    }

    private static AdminPetImageListItemViewModel ToListItem(PetImage image, string? petName, string? ownerName)
    {
        return new AdminPetImageListItemViewModel
        {
            Id = image.Id,
            PetId = image.PetId,
            PetName = petName ?? string.Empty,
            OwnerName = ownerName,
            ImageUrl = image.ImageUrl,
            IsAvatar = image.IsAvatar,
            UploadedAt = image.UploadedAt
        };
    }

    private List<string> GetValidationErrors()
    {
        return ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "Dữ liệu không hợp lệ."
                : error.ErrorMessage)
            .ToList();
    }
}
