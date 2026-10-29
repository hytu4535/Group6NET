using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IFeedbackRepository
{
    Task<List<Feedback>> GetAllAsync();
    Task<PagedResult<Feedback>> GetPagedAsync(int page, int pageSize);
    Task<Feedback?> GetByIdAsync(int id);
    Task<List<Feedback>> GetByUserIdAsync(int userId);
    Task<PagedResult<Feedback>> GetByUserIdPagedAsync(int userId, int page, int pageSize);
    Task AddAsync(Feedback feedback);
    Task UpdateStatusAsync(int id, string newStatus);
}