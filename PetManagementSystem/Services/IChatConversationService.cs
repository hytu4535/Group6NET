using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IChatConversationService
{
    Task<List<ChatConversation>> GetAllAsync(string? search, DateTime? fromDate, DateTime? toDate);
    Task<PagedResult<ChatConversation>> GetPagedAsync(string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<ChatConversation?> GetByIdAsync(int id);
    Task<bool> DeleteAsync(int id);
}