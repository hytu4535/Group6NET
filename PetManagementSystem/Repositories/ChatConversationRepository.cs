using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class ChatConversationRepository(AppDbContext dbContext) : IChatConversationRepository
{
    public async Task<List<ChatConversation>> GetAllAsync(string? search, DateTime? fromDate, DateTime? toDate)
    {
        var query = dbContext.ChatConversations
            .Include(conversation => conversation.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(conversation =>
                (conversation.User != null &&
                 (conversation.User.Username.Contains(keyword) ||
                  (conversation.User.FullName != null && conversation.User.FullName.Contains(keyword)))) ||
                conversation.UserRequest.Contains(keyword) ||
                conversation.AiResponse.Contains(keyword));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(conversation => conversation.CreatedAt >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(conversation => conversation.CreatedAt < toDate.Value.Date.AddDays(1));
        }

        return await query
            .OrderByDescending(conversation => conversation.CreatedAt)
            .ThenByDescending(conversation => conversation.Id)
            .ToListAsync();
    }

    public async Task<PagedResult<ChatConversation>> GetPagedAsync(
        string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = dbContext.ChatConversations
            .Include(conversation => conversation.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(conversation =>
                (conversation.User != null &&
                 (conversation.User.Username.Contains(keyword) ||
                  (conversation.User.FullName != null && conversation.User.FullName.Contains(keyword)))) ||
                conversation.UserRequest.Contains(keyword) ||
                conversation.AiResponse.Contains(keyword));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(conversation => conversation.CreatedAt >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(conversation => conversation.CreatedAt < toDate.Value.Date.AddDays(1));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(conversation => conversation.CreatedAt)
            .ThenByDescending(conversation => conversation.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ChatConversation>(items, page, pageSize, totalItems);
    }

    public Task<ChatConversation?> GetByIdAsync(int id)
    {
        return dbContext.ChatConversations
            .Include(conversation => conversation.User)
            .FirstOrDefaultAsync(conversation => conversation.Id == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var conversation = await dbContext.ChatConversations.FindAsync(id);
        if (conversation is null)
        {
            return false;
        }

        dbContext.ChatConversations.Remove(conversation);
        await dbContext.SaveChangesAsync();
        return true;
    }
}