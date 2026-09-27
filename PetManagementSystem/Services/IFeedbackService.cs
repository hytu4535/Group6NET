using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IFeedbackService
{
    Task<List<Feedback>> GetAllFeedbacksAsync();
    Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize);
    Task<Feedback?> GetFeedbackByIdAsync(int id);
    Task<List<Feedback>> GetMyFeedbacksAsync();
    Task<(bool Success, string ErrorMessage)> CreateFeedbackAsync(string comment);
    Task<(bool Success, string ErrorMessage)> UpdateStatusAsync(int id, string status);
}