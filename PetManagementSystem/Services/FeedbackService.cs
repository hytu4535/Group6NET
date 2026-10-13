using System.Security.Claims;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class FeedbackService(
    IFeedbackRepository feedbackRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<FeedbackService> logger) : IFeedbackService
{
    public async Task<List<Feedback>> GetAllFeedbacksAsync()
    {
        try
        {
            return await feedbackRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách feedback.");
            return [];
        }
    }

    public async Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize)
    {
        try
        {
            return await feedbackRepository.GetPagedAsync(page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy trang danh sách feedback.");
            return new PagedResult<Feedback>([], page, pageSize, 0);
        }
    }

    public async Task<Feedback?> GetFeedbackByIdAsync(int id)
    {
        try
        {
            return await feedbackRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi lấy thông tin feedback ID: {FeedbackId}", id);
            return null;
        }
    }

    public async Task<List<Feedback>> GetMyFeedbacksAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return [];
            }

            return await feedbackRepository.GetByUserIdAsync(userId.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy feedback cá nhân.");
            return [];
        }
    }

    public async Task<PagedResult<Feedback>> GetMyFeedbacksPagedAsync(int page, int pageSize)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new InvalidOperationException("Không xác định được tài khoản đang đăng nhập.");
        }

        try
        {
            return await feedbackRepository.GetByUserIdPagedAsync(userId.Value, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi lấy danh sách feedback phân trang của user {UserId}.", userId.Value);
            throw;
        }
    }

    public async Task<(bool Success, string ErrorMessage)> CreateFeedbackAsync(string comment)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return (false, "Bạn cần đăng nhập để gửi phản hồi.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return (false, "Nội dung phản hồi không được để trống.");
            }

            var feedback = new Feedback
            {
                UserId = userId.Value,
                Comment = comment.Trim(),
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await feedbackRepository.AddAsync(feedback);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi gửi phản hồi mới.");
            return (false, "Không thể gửi phản hồi do lỗi hệ thống.");
        }
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateStatusAsync(int id, string status)
    {
        try
        {
            var normalizedStatus = status.Trim().ToLower();
            if (normalizedStatus != "pending" && normalizedStatus != "resolved")
            {
                return (false, "Trạng thái không hợp lệ (Chỉ nhận 'pending' hoặc 'resolved').");
            }

            var feedback = await feedbackRepository.GetByIdAsync(id);
            if (feedback is null)
            {
                return (false, "Không tìm thấy phản hồi cần cập nhật.");
            }

            await feedbackRepository.UpdateStatusAsync(id, normalizedStatus);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi hệ thống khi cập nhật trạng thái feedback ID: {FeedbackId}", id);
            return (false, "Không thể cập nhật trạng thái do lỗi hệ thống.");
        }
    }

    private int? GetCurrentUserId()
    {
        var claimValue = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}