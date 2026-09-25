using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/main/notifications")]
public class NotificationsApiController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<Notification>>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<Notification>>.Fail(error));
        }

        var result = await notificationService.GetCustomerNotificationsPagedAsync(page, pageSize);
        return Ok(ApiResponse<IReadOnlyList<Notification>>.Ok(
            result.Items, "Lấy danh sách thông báo thành công.", result.Page, result.PageSize, result.TotalItems));
    }

    private static bool IsValidPage(int page, int pageSize, out string error)
    {
        error = page < 1 ? "page phải lớn hơn hoặc bằng 1." :
            pageSize is < 1 or > 100 ? "pageSize phải trong khoảng từ 1 đến 100." : string.Empty;
        return error.Length == 0;
    }
}