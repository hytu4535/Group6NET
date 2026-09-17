using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class StaffRepository(AppDbContext dbContext) : IStaffRepository
{
    public async Task<List<Staff>> GetAllWithUserAsync()
    {
        return await dbContext.Staff
            .Include(x => x.User)
                .ThenInclude(u => u!.Role)
            .Where(x => x.Status == "active" && x.User != null && x.User.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await dbContext.Staff
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId, int? excludeId = null)
    {
        var query = dbContext.Staff.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(Staff staff)
    {
        await dbContext.Staff.AddAsync(staff);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Staff staff)
    {
        dbContext.Staff.Update(staff);
        await dbContext.SaveChangesAsync();
    }
}
