using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class ClientAiChatService(IChatConversationRepository chatConversationRepository) : IClientAiChatService
{
    public Task<PagedResult<ChatConversation>> GetHistoryAsync(int userId, int page, int pageSize)
    {
        return chatConversationRepository.GetByUserIdPagedAsync(userId, page, pageSize);
    }

    public Task<ServiceResult<string>> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.FromResult(ServiceResult<string>.Fail("Vui lòng nhập nội dung tin nhắn.", "message"));
        }

        return Task.FromResult(ServiceResult<string>.Fail(
            "Trợ lý AI chưa được kết nối. Tin nhắn chưa được gửi và chưa lưu vào lịch sử."));
    }
}
