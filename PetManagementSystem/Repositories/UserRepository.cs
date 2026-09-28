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
            .FirstOrDefaultAsync(x => x.Username.Trim() == normalized);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetUserWithRoleAndPermissionsAsync(string username)
    {
        var normalized = username?.Trim() ?? string.Empty;
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Username.Trim() == normalized);

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

    public async Task<List<User>> GetAllWithRoleAsync()
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .Where(x => x.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
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
