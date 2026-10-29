using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class FeedbackRepository(AppDbContext dbContext) : IFeedbackRepository
{
    public async Task<List<Feedback>> GetAllAsync()
    {
        return await dbContext.Feedbacks
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<PagedResult<Feedback>> GetPagedAsync(int page, int pageSize)
    {
        var query = dbContext.Feedbacks
            .Include(x => x.User)
            .AsNoTracking();
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Feedback>(items, totalItems, page, pageSize);
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await dbContext.Feedbacks
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Feedback>> GetByUserIdAsync(int userId)
    {
        return await dbContext.Feedbacks
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<Feedback>> GetByUserIdPagedAsync(int userId, int page, int pageSize)
    {
        var query = dbContext.Feedbacks
            .AsNoTracking()
            .Where(feedback => feedback.UserId == userId);
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(feedback => feedback.CreatedAt)
            .ThenByDescending(feedback => feedback.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Feedback>(items, totalItems, page, pageSize);
    }

    public async Task AddAsync(Feedback feedback)
    {
        feedback.CreatedAt = DateTime.Now;
        if (string.IsNullOrEmpty(feedback.Status))
        {
            feedback.Status = "pending";
        }
        await dbContext.Feedbacks.AddAsync(feedback);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, string newStatus)
    {
        var feedback = await dbContext.Feedbacks.FindAsync(id);
        if (feedback != null)
        {
            feedback.Status = newStatus;
            await dbContext.SaveChangesAsync();
        }
    }
}