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
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Pet?> GetByTokenAsync(string token)
    {
        var pet = await dbContext.Pets
            .Include(x => x.User)
            .Include(x => x.PetImages)
            .Include(x => x.PetHealthRecords)
            .Include(x => x.VaccinationRecords)
            .FirstOrDefaultAsync(x => x.QrToken == token);

        if (pet != null && string.IsNullOrWhiteSpace(pet.QrToken))
        {
            pet.QrToken = Guid.NewGuid().ToString("N");
            dbContext.Pets.Update(pet);
            await dbContext.SaveChangesAsync();
        }

        return pet;
    }

    public async Task<List<Pet>> GetByUserIdAsync(int userId)
    {
        return await dbContext.Pets
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Pet pet)
    {
        pet.CreatedAt = DateTime.Now;
        if (string.IsNullOrEmpty(pet.QrToken))
        {
            pet.QrToken = Guid.NewGuid().ToString("N");
        }
        await dbContext.Pets.AddAsync(pet);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pet pet)
    {
        pet.UpdatedAt = DateTime.Now;
        dbContext.Pets.Update(pet);
        await dbContext.SaveChangesAsync();
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