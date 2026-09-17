using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class PermissionRepository(AppDbContext dbContext) : IPermissionRepository
{
    public async Task<List<Permission>> GetAllAsync()
    {
        return await dbContext.Permissions
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsCodeAsync(string code, int? excludeId = null)
    {
        var query = dbContext.Permissions.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Code == code);
    }

    public async Task AddAsync(Permission permission)
    {
        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Permission permission)
    {
        dbContext.Permissions.Update(permission);
        await dbContext.SaveChangesAsync();
    }
}
