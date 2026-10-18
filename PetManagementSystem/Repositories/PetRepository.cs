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

    public async Task<PagedResult<Pet>> GetPagedByUserIdAsync(int userId, int page, int pageSize)
    {
        var query = dbContext.Pets
            .AsNoTracking()
            .Where(pet => pet.UserId == userId);
        var totalItems = await query.CountAsync();
        var items = await query
            .Include(pet => pet.PetImages)
            .OrderByDescending(pet => pet.CreatedAt)
            .ThenByDescending(pet => pet.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Pet>(items, totalItems, page, pageSize);
    }

    public Task<Pet?> GetByIdAndUserIdAsync(int petId, int userId)
    {
        return dbContext.Pets
            .Include(pet => pet.PetImages)
            .FirstOrDefaultAsync(pet => pet.Id == petId && pet.UserId == userId);
    }

    public Task<bool> HasAppointmentsAsync(int petId)
    {
        return dbContext.Appointments.AnyAsync(appointment => appointment.PetId == petId);
    }

    public async Task AddClientPetAsync(Pet pet)
    {
        pet.CreatedAt = DateTime.UtcNow;
        pet.QrToken = Guid.NewGuid().ToString("N");
        await dbContext.Pets.AddAsync(pet);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateClientPetAsync(Pet pet)
    {
        pet.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteClientPetAsync(Pet pet)
    {
        dbContext.Pets.Remove(pet);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddPetImageAsync(PetImage image, bool setAsAvatar)
    {
        var petImages = await dbContext.PetImages
            .Where(existing => existing.PetId == image.PetId)
            .ToListAsync();
        if (setAsAvatar || petImages.All(existing => !existing.IsAvatar))
        {
            foreach (var existing in petImages)
            {
                existing.IsAvatar = false;
            }
            image.IsAvatar = true;
        }

        await dbContext.PetImages.AddAsync(image);
        await dbContext.SaveChangesAsync();
    }

    public Task<PetImage?> GetPetImageAsync(int imageId, int petId, int userId)
    {
        return dbContext.PetImages
            .Include(image => image.Pet)
            .FirstOrDefaultAsync(image =>
                image.Id == imageId &&
                image.PetId == petId &&
                image.Pet != null &&
                image.Pet.UserId == userId);
    }

    public async Task DeletePetImageAsync(PetImage image)
    {
        dbContext.PetImages.Remove(image);
        if (image.IsAvatar)
        {
            var nextAvatar = await dbContext.PetImages
                .Where(existing => existing.PetId == image.PetId && existing.Id != image.Id)
                .OrderBy(existing => existing.UploadedAt)
                .FirstOrDefaultAsync();
            if (nextAvatar is not null)
            {
                nextAvatar.IsAvatar = true;
            }
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task SetPetImageAsAvatarAsync(PetImage image)
    {
        var petImages = await dbContext.PetImages
            .Where(existing => existing.PetId == image.PetId)
            .ToListAsync();
        foreach (var existing in petImages)
        {
            existing.IsAvatar = existing.Id == image.Id;
        }
        await dbContext.SaveChangesAsync();
    }
}