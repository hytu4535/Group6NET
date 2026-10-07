using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Notifications;

namespace PetManagementSystem.Controllers.Api;

[Authorize(Roles = "member")]
[Route("api/client/notifications")]
public class ClientNotificationsApiController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClientNotificationViewModel>>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<ClientNotificationViewModel>>.Fail(
                "Vui lòng đăng nhập lại."));
        }
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<ClientNotificationViewModel>>.Fail(error));
        }

        var result = await notificationService.GetByUserIdPagedAsync(userId, page, pageSize);
        return Ok(ApiResponse<IReadOnlyList<ClientNotificationViewModel>>.Ok(
            result.Items.Select(notification => new ClientNotificationViewModel
            {
                Id = notification.Id,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            }).ToList(),
            "Lấy danh sách thông báo thành công.",
            result.Page,
            result.PageSize,
            result.TotalItems));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<int>.Fail("Vui lòng đăng nhập lại."));
        }

        var unreadCount = await notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(unreadCount, "Lấy số thông báo chưa đọc thành công."));
    }

    [HttpPut("{id:int}/read")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Vui lòng đăng nhập lại."));
        }

        return await notificationService.MarkAsReadAsync(userId, id)
            ? Ok(ApiResponse<object>.Ok(new { id }, "Đã đánh dấu thông báo là đã đọc."))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy thông báo."));
    }

    [HttpPut("read-all")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Vui lòng đăng nhập lại."));
        }

        await notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse<object>.Ok(new { }, "Đã đánh dấu tất cả thông báo là đã đọc."));
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private static bool IsValidPage(int page, int pageSize, out string error)
    {
        error = page < 1 ? "page phải lớn hơn hoặc bằng 1." :
            pageSize is < 1 or > 100 ? "pageSize phải trong khoảng từ 1 đến 100." : string.Empty;
        return error.Length == 0;
    }
}
