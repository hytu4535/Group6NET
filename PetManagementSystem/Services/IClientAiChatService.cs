using PetManagementSystem.Models;
using PetManagementSystem.Services;

namespace PetManagementSystem.Services;

public interface IClientAiChatService
{
    Task<PagedResult<ChatConversation>> GetHistoryAsync(int userId, int page, int pageSize);
    Task<ServiceResult<string>> SendMessageAsync(string message);
}
