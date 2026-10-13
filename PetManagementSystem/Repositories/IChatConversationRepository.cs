using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface IChatConversationRepository
{
    Task<List<ChatConversation>> GetAllAsync(string? search, DateTime? fromDate, DateTime? toDate);
    Task<PagedResult<ChatConversation>> GetPagedAsync(string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<PagedResult<ChatConversation>> GetByUserIdPagedAsync(int userId, int page, int pageSize);
    Task<ChatConversation?> GetByIdAsync(int id);
    Task<bool> DeleteAsync(int id);
}