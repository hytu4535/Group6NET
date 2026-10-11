using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class RoleRepository(AppDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        return await dbContext.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == roleId);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await dbContext.Roles
            .FirstOrDefaultAsync(x => x.Name.ToLower() == roleName.ToLower());
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await dbContext.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(x => x.Status == "active")
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
    }

    public async Task<bool> ExistsNameAsync(string roleName, int? excludeId = null)
    {
        var query = dbContext.Roles.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Name == roleName);
    }

    public async Task AddAsync(Role role, IEnumerable<int> permissionIds)
    {
        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        var rolePermissions = permissionIds
            .Distinct()
            .Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (rolePermissions.Count > 0)
        {
            await dbContext.RolePermissions.AddRangeAsync(rolePermissions);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Role role, IEnumerable<int> permissionIds)
    {
        dbContext.Roles.Update(role);

        var existing = dbContext.RolePermissions.Where(x => x.RoleId == role.Id);
        dbContext.RolePermissions.RemoveRange(existing);

        var rolePermissions = permissionIds
            .Distinct()
            .Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (rolePermissions.Count > 0)
        {
            await dbContext.RolePermissions.AddRangeAsync(rolePermissions);
        }

        await dbContext.SaveChangesAsync();
    }
}
