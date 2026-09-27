using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class CategoryRepository(AppDbContext dbContext) : ICategoryRepository
{
    public async Task<PetManagementSystem.Helpers.PagedResult<Category>> SearchAsync(string? search, int page, int pageSize)
    {
        var query = dbContext.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.CategoryName.Contains(keyword)
                                     || (x.Description != null && x.Description.Contains(keyword)));
        }

        return await query
            .OrderBy(x => x.CategoryName)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.CategoryName)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsNameAsync(string name, int? excludeId = null)
    {
        var query = dbContext.Categories.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.CategoryName == name);
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await dbContext.Products.AnyAsync(x => x.CategoryId == id);
    }

    public async Task<Dictionary<int, int>> GetProductCountsAsync(IEnumerable<int> categoryIds)
    {
        var ids = categoryIds.ToList();
        return await dbContext.Products
            .Where(x => ids.Contains(x.CategoryId))
            .GroupBy(x => x.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);
    }

    public async Task AddAsync(Category category)
    {
        await dbContext.Categories.AddAsync(category);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(category).State == EntityState.Detached)
        {
            dbContext.Categories.Update(category);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();
    }
}