using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        var normalized = username?.Trim() ?? string.Empty;
        return await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => (x.Username ?? string.Empty).Trim() == normalized);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalized = email?.Trim() ?? string.Empty;
        return await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => (x.Email ?? string.Empty).Trim() == normalized);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Staff)
            .Include(x => x.Veterinarian)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetUserWithRoleAndPermissionsAsync(string username)
    {
        var normalized = username?.Trim() ?? string.Empty;
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => (x.Username ?? string.Empty).Trim() == normalized);

        if (user is null)
        {
            return null;
        }

        if (user.RoleId > 0)
        {
            user.Role = await dbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == user.RoleId);
        }

        return user;
    }

    public async Task<List<User>> GetAllWithRoleAsync(bool includeInactive = false)
    {
        var query = dbContext.Users
            .Include(x => x.Role)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.Status == "active");
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<bool> ExistsUsernameAsync(string username, int? excludeId = null)
    {
        var query = dbContext.Users.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Username == username);
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
    {
        var query = dbContext.Users.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Email == email);
    }

    public async Task<bool> HasRelatedDataAsync(int userId)
    {
        var user = await dbContext.Users
            .Include(x => x.Staff)
            .Include(x => x.Veterinarian)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            return false;
        }

        return await dbContext.Pets.AnyAsync(x => x.UserId == userId)
            || await dbContext.Appointments.AnyAsync(x => x.UserId == userId || (user.Veterinarian != null && x.VetId == user.Veterinarian.Id))
            || await dbContext.Orders.AnyAsync(x => x.UserId == userId)
            || await dbContext.Carts.AnyAsync(x => x.UserId == userId)
            || await dbContext.Feedbacks.AnyAsync(x => x.UserId == userId)
            || await dbContext.Notifications.AnyAsync(x => x.UserId == userId)
            || await dbContext.ChatConversations.AnyAsync(x => x.UserId == userId)
            || (user.Staff != null && await dbContext.PetRecognitionHistories.AnyAsync(x => x.StaffId == user.Staff.Id));
    }

    public async Task DeleteHardAsync(User user)
    {
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddAsync(User user)
    {
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
    }
}
