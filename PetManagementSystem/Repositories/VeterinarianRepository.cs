using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class VeterinarianRepository(AppDbContext dbContext) : IVeterinarianRepository
{
    public async Task<List<Veterinarian>> GetAllWithUserAsync()
    {
        return await dbContext.Veterinarians
            .Include(x => x.User)
                .ThenInclude(u => u!.Role)
            .Where(x => x.Status == "active" && x.User != null && x.User.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Veterinarian?> GetByIdAsync(int id)
    {
        return await dbContext.Veterinarians
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId, int? excludeId = null)
    {
        var query = dbContext.Veterinarians.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(Veterinarian veterinarian)
    {
        await dbContext.Veterinarians.AddAsync(veterinarian);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Veterinarian veterinarian)
    {
        dbContext.Veterinarians.Update(veterinarian);
        await dbContext.SaveChangesAsync();
    }
}
