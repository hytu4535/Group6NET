using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.AI;

namespace PetManagementSystem.Controllers.Api;

[ApiController]
[Authorize(Roles = "member")]
[Route("api/client/ai-chat")]
public class ClientAiChatApiController(IClientAiChatService aiChatService) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AiChatConversationViewModel>>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<IReadOnlyList<AiChatConversationViewModel>>.Fail(
                "Vui lòng đăng nhập lại."));
        }
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<AiChatConversationViewModel>>.Fail(error));
        }

        var result = await aiChatService.GetHistoryAsync(userId, page, pageSize);
        var conversations = result.Items.Select(conversation => new AiChatConversationViewModel
        {
            Id = conversation.Id,
            Title = conversation.Title,
            UserMessage = conversation.UserRequest,
            AiMessage = conversation.AiResponse,
            CreatedAt = conversation.CreatedAt
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<AiChatConversationViewModel>>.Ok(
            conversations,
            "Lấy lịch sử chat thành công.",
            result.Page,
            result.PageSize,
            result.TotalItems));
    }

    [HttpPost("messages")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<object>>> SendMessage(
        [FromBody] SendAiChatMessageViewModel model)
    {
        if (!TryGetUserId(out _))
        {
            return Unauthorized(ApiResponse<object>.Fail("Vui lòng đăng nhập lại."));
        }
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Dữ liệu không hợp lệ."
                    : error.ErrorMessage)
                .ToArray();
            return BadRequest(ApiResponse<object>.Fail("Tin nhắn không hợp lệ.", errors));
        }

        var result = await aiChatService.SendMessageAsync(model.Message);
        if (!result.Success)
        {
            return result.Field is not null
                ? BadRequest(ApiResponse<object>.Fail(result.Error ?? "Tin nhắn không hợp lệ."))
                : StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Fail(result.Error ?? "Dịch vụ AI hiện không khả dụng."));
        }

        return Ok(ApiResponse<object>.Ok(new { reply = result.Data }, "AI đã trả lời."));
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
