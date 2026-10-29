using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Feedbacks;

namespace PetManagementSystem.Controllers.Api;

[Authorize(Roles = "member")]
[Route("api/client/feedbacks")]
public class ClientFeedbacksApiController(IFeedbackService feedbackService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FeedbackListItemViewModel>>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!IsValidPage(page, pageSize, out var error))
        {
            return BadRequest(ApiResponse<IReadOnlyList<FeedbackListItemViewModel>>.Fail(error));
        }

        var result = await feedbackService.GetMyFeedbacksPagedAsync(page, pageSize);
        return Ok(ApiResponse<IReadOnlyList<FeedbackListItemViewModel>>.Ok(
            result.Items.Select(feedback => new FeedbackListItemViewModel
            {
                Id = feedback.Id,
                Comment = feedback.Comment,
                Status = feedback.Status,
                CreatedAt = feedback.CreatedAt
            }).ToList(),
            "Lấy lịch sử phản hồi thành công.",
            result.Page,
            result.PageSize,
            result.TotalItems));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ApiResponse<FeedbackListItemViewModel>>> Create(
        [FromBody] CreateFeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Dữ liệu không hợp lệ."
                    : error.ErrorMessage)
                .ToArray();
            return BadRequest(ApiResponse<FeedbackListItemViewModel>.Fail(
                "Nội dung phản hồi không hợp lệ.", errors));
        }

        var (success, errorMessage) = await feedbackService.CreateFeedbackAsync(model.Comment);
        if (!success)
        {
            return BadRequest(ApiResponse<FeedbackListItemViewModel>.Fail(errorMessage));
        }

        var feedback = new FeedbackListItemViewModel
        {
            Comment = model.Comment.Trim(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
        return Ok(ApiResponse<FeedbackListItemViewModel>.Ok(feedback, "Gửi phản hồi thành công."));
    }

    private static bool IsValidPage(int page, int pageSize, out string error)
    {
        error = page < 1 ? "page phải lớn hơn hoặc bằng 1." :
            pageSize is < 1 or > 100 ? "pageSize phải trong khoảng từ 1 đến 100." : string.Empty;
        return error.Length == 0;
    }
}
