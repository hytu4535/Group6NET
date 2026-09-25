using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<PagedResult<Product>> SearchAsync(string? search, int? categoryId, string? status, int page, int pageSize)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.ProductName.Contains(keyword)
                                     || (x.Unit != null && x.Unit.Contains(keyword)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<Product>> GetOptionsAsync(bool onlyActive)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();
        if (onlyActive)
        {
            query = query.Where(x => x.Status == ProductStatuses.Active);
        }

        return await query
            .OrderBy(x => x.ProductName)
            .Select(x => new Product
            {
                Id = x.Id,
                ProductName = x.ProductName,
                Unit = x.Unit,
                ImportPrice = x.ImportPrice,
                SellPrice = x.SellPrice,
                StockQuantity = x.StockQuantity,
                Status = x.Status
            })
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await dbContext.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        return await dbContext.Products
            .Where(x => idList.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<bool> ExistsNameAsync(string name, int categoryId, int? excludeId = null)
    {
        var query = dbContext.Products.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.CategoryId == categoryId && x.ProductName == name);
    }

    public async Task<bool> IsReferencedAsync(int id)
    {
        return await dbContext.ImportDetails.AnyAsync(x => x.ProductId == id)
               || await dbContext.OrderItems.AnyAsync(x => x.ProductId == id)
               || await dbContext.CartItems.AnyAsync(x => x.ProductId == id);
    }

    public async Task AddAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(product).State == EntityState.Detached)
        {
            dbContext.Products.Update(product);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }
}
