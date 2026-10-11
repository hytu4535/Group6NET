using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class PetRepository(AppDbContext dbContext) : IPetRepository
{
    public async Task<List<Pet>> GetAllAsync(string? searchKeyword = null)
    {
        var query = dbContext.Pets
            .Include(x => x.User)
            .Include(x => x.PetImages)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.Species.ToLower().Contains(keyword) ||
                (x.Breed != null && x.Breed.ToLower().Contains(keyword)));
        }

        return await query.OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<PagedResult<Pet>> GetPagedAsync(string? searchKeyword, int page, int pageSize)
    {
        var query = dbContext.Pets
            .Include(x => x.User)
            .Include(x => x.PetImages)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.Species.ToLower().Contains(keyword) ||
                (x.Breed != null && x.Breed.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Pet>(items, page, pageSize, totalItems);
    }

    public async Task<Pet?> GetByIdAsync(int id)
    {
        return await dbContext.Pets
            .Include(x => x.User)
            .Include(x => x.PetImages)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Pet>> GetByUserIdAsync(int userId)
    {
        return await dbContext.Pets
            .Include(x => x.User)
            .Include(x => x.PetImages)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Pet pet, string? avatarUrl = null)
    {
        pet.CreatedAt = DateTime.Now;
        if (string.IsNullOrEmpty(pet.QrToken))
        {
            pet.QrToken = Guid.NewGuid().ToString("N");
        }
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            pet.PetImages.Add(new PetImage
            {
                ImageUrl = avatarUrl,
                IsAvatar = true,
                UploadedAt = DateTime.UtcNow
            });
        }

        await dbContext.Pets.AddAsync(pet);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pet pet, string? avatarUrl = null)
    {
        pet.UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            foreach (var existingAvatar in pet.PetImages.Where(image => image.IsAvatar))
            {
                existingAvatar.IsAvatar = false;
            }

            pet.PetImages.Add(new PetImage
            {
                PetId = pet.Id,
                ImageUrl = avatarUrl,
                IsAvatar = true,
                UploadedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> SetAvatarAsync(int petId, int imageId)
    {
        var images = await dbContext.PetImages
            .Where(image => image.PetId == petId)
            .ToListAsync();
        if (images.All(image => image.Id != imageId))
        {
            return false;
        }

        foreach (var image in images)
        {
            image.IsAvatar = image.Id == imageId;
        }

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var pet = await dbContext.Pets.FindAsync(id);
        if (pet != null)
        {
            dbContext.Pets.Remove(pet);
            await dbContext.SaveChangesAsync();
        }
    }
}