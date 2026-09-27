using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class ChatConversationService(IChatConversationRepository repository) : IChatConversationService
{
    public Task<List<ChatConversation>> GetAllAsync(string? search, DateTime? fromDate, DateTime? toDate)
    {
        return repository.GetAllAsync(search, fromDate, toDate);
    }

    public Task<PagedResult<ChatConversation>> GetPagedAsync(
        string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        return repository.GetPagedAsync(search, fromDate, toDate, page, pageSize);
    }

    public Task<ChatConversation?> GetByIdAsync(int id)
    {
        return repository.GetByIdAsync(id);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return repository.DeleteAsync(id);
    }
}