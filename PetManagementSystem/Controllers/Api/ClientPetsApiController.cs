using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Pets;

namespace PetManagementSystem.Controllers.Api;

[Authorize(Roles = "member")]
[Route("api/client/pets")]
public class ClientPetsApiController(IPetService petService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PetListItemViewModel>>>> GetPets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<PetListItemViewModel>>.Fail(
                "Không xác định được người dùng.", "Vui lòng đăng nhập lại."));
        }
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<PetListItemViewModel>>.Fail(error));
        }

        var result = await petService.GetClientPetsPagedAsync(userId, page, pageSize);
        var pets = result.Items.Select(ToListItem).ToList();
        return Ok(ApiResponse<IReadOnlyList<PetListItemViewModel>>.Ok(
            pets, "Lấy danh sách thú cưng thành công.", result.Page, result.PageSize, result.TotalItems));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PetDetailsViewModel>>> GetPet(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<PetDetailsViewModel>.Fail("Không xác định được người dùng."));
        }

        var pet = await petService.GetClientPetAsync(userId, id);
        return pet is null
            ? NotFound(ApiResponse<PetDetailsViewModel>.Fail("Không tìm thấy thú cưng."))
            : Ok(ApiResponse<PetDetailsViewModel>.Ok(ToDetails(pet), "Lấy thông tin thú cưng thành công."));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<PetDetailsViewModel>>> CreatePet(
        [FromForm] PetUpsertViewModel model)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<PetDetailsViewModel>.Fail("Không xác định được người dùng."));
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(
                "Dữ liệu thú cưng không hợp lệ.", [.. GetValidationErrors()]));
        }
        if (model.Image is { Length: > 0 })
        {
            try
            {
                await petService.ValidateClientPetImageAsync(model.Image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(ex.Message));
            }
        }

        var pet = await petService.CreateClientPetAsync(userId, ToPet(model));
        if (model.Image is { Length: > 0 })
        {
            try
            {
                await petService.AddClientPetImageAsync(userId, pet.Id, model.Image, setAsAvatar: true);
                pet = await petService.GetClientPetAsync(userId, pet.Id) ?? pet;
            }
            catch (ArgumentException ex)
            {
                await petService.DeleteClientPetAsync(userId, pet.Id);
                return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(ex.Message));
            }
        }

        return CreatedAtAction(
            nameof(GetPet),
            new { id = pet.Id },
            ApiResponse<PetDetailsViewModel>.Ok(ToDetails(pet), "Thêm thú cưng thành công."));
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<PetDetailsViewModel>>> UpdatePet(
        int id,
        [FromForm] PetUpsertViewModel model)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<PetDetailsViewModel>.Fail("Không xác định được người dùng."));
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(
                "Dữ liệu thú cưng không hợp lệ.", [.. GetValidationErrors()]));
        }
        if (model.Image is { Length: > 0 })
        {
            try
            {
                await petService.ValidateClientPetImageAsync(model.Image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(ex.Message));
            }
        }

        var pet = await petService.UpdateClientPetAsync(userId, id, ToPet(model));
        if (pet is null)
        {
            return NotFound(ApiResponse<PetDetailsViewModel>.Fail("Không tìm thấy thú cưng."));
        }

        if (model.Image is { Length: > 0 })
        {
            try
            {
                await petService.AddClientPetImageAsync(userId, id, model.Image, setAsAvatar: true);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<PetDetailsViewModel>.Fail(ex.Message));
            }
        }

        pet = await petService.GetClientPetAsync(userId, id) ?? pet;
        return Ok(ApiResponse<PetDetailsViewModel>.Ok(ToDetails(pet), "Cập nhật thú cưng thành công."));
    }

    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> DeletePet(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng."));
        }

        return await petService.DeleteClientPetAsync(userId, id)
            ? Ok(ApiResponse<object>.Ok(new { id }, "Xóa thú cưng thành công."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy thú cưng."));
    }

    [HttpGet("{id:int}/images")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PetImageViewModel>>>> GetPetImages(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<PetImageViewModel>>.Fail("Không xác định được người dùng."));
        }

        var images = await petService.GetClientPetImagesAsync(userId, id);
        return images is null
            ? NotFound(ApiResponse<IReadOnlyList<PetImageViewModel>>.Fail("Không tìm thấy thú cưng."))
            : Ok(ApiResponse<IReadOnlyList<PetImageViewModel>>.Ok(
                images.Select(ToImage).ToList(), "Lấy ảnh thú cưng thành công."));
    }

    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<PetImageViewModel>>> UploadPetImage(
        int id,
        [FromForm] PetImageUploadViewModel model)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<PetImageViewModel>.Fail("Không xác định được người dùng."));
        }
        if (!ModelState.IsValid || model.Image is null)
        {
            return BadRequest(ApiResponse<PetImageViewModel>.Fail(
                "Vui lòng chọn ảnh hợp lệ.", [.. GetValidationErrors()]));
        }

        try
        {
            var image = await petService.AddClientPetImageAsync(userId, id, model.Image, model.SetAsAvatar);
            return image is null
                ? NotFound(ApiResponse<PetImageViewModel>.Fail("Không tìm thấy thú cưng."))
                : Ok(ApiResponse<PetImageViewModel>.Ok(ToImage(image), "Tải ảnh lên thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PetImageViewModel>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> DeletePetImage(int id, int imageId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng."));
        }

        return await petService.DeleteClientPetImageAsync(userId, id, imageId)
            ? Ok(ApiResponse<object>.Ok(new { id = imageId }, "Xóa ảnh thành công."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy ảnh thú cưng."));
    }

    [HttpPut("{id:int}/images/{imageId:int}/avatar")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> SetPetImageAsAvatar(int id, int imageId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Không xác định được người dùng."));
        }

        return await petService.SetClientPetImageAsAvatarAsync(userId, id, imageId)
            ? Ok(ApiResponse<object>.Ok(new { id = imageId }, "Đã cập nhật ảnh đại diện."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy ảnh thú cưng."));
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private List<string> GetValidationErrors()
    {
        return ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Dữ liệu không hợp lệ." : error.ErrorMessage)
            .ToList();
    }

    private static bool IsValidPage(int page, int pageSize, out string error)
    {
        error = page < 1 ? "page phải lớn hơn hoặc bằng 1." :
            pageSize is < 1 or > 100 ? "pageSize phải trong khoảng từ 1 đến 100." : string.Empty;
        return error.Length == 0;
    }

    private static Pet ToPet(PetUpsertViewModel model)
    {
        return new Pet
        {
            Name = model.Name,
            Species = model.Species,
            Breed = model.Breed,
            Gender = model.Gender,
            WeightKg = model.WeightKg
        };
    }

    private static PetListItemViewModel ToListItem(Pet pet)
    {
        return new PetListItemViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            Gender = pet.Gender,
            WeightKg = pet.WeightKg,
            AvatarUrl = pet.PetImages.FirstOrDefault(image => image.IsAvatar)?.ImageUrl
                ?? pet.PetImages.OrderByDescending(image => image.CreatedAt).FirstOrDefault()?.ImageUrl
        };
    }

    private static PetDetailsViewModel ToDetails(Pet pet)
    {
        return new PetDetailsViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            Gender = pet.Gender,
            WeightKg = pet.WeightKg,
            CreatedAt = pet.CreatedAt,
            UpdatedAt = pet.UpdatedAt,
            Images = pet.PetImages
                .OrderByDescending(image => image.CreatedAt)
                .Select(ToImage)
                .ToList()
        };
    }

    private static PetImageViewModel ToImage(PetImage image)
    {
        return new PetImageViewModel
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            IsAvatar = image.IsAvatar,
            CreatedAt = image.CreatedAt
        };
    }
}
