using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/main/pets")]
public class PetsApiController(IPetService petService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Pet>>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<Pet>>.Fail(error));
        }

        var result = await petService.GetPagedPetsAsync(search, page, pageSize);
        return Ok(ApiResponse<IReadOnlyList<Pet>>.Ok(
            result.Items, "Lấy danh sách thú cưng thành công.", result.Page, result.PageSize, result.TotalItems));
    }

    private static bool IsValidPage(int page, int pageSize, out string error)
    {
        error = page < 1 ? "page phải lớn hơn hoặc bằng 1." :
            pageSize is < 1 or > 100 ? "pageSize phải trong khoảng từ 1 đến 100." : string.Empty;
        return error.Length == 0;
    }
}