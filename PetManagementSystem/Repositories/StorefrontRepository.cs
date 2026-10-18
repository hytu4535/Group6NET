using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class StorefrontRepository(AppDbContext dbContext) : IStorefrontRepository
{
    public async Task<PagedResult<Product>> SearchAsync(ProductFilter filter, int page, int pageSize)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Status == ProductStatuses.Active);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var keyword = filter.Search.Trim();
            query = query.Where(x => x.ProductName.Contains(keyword));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.SellPrice >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.SellPrice <= filter.MaxPrice.Value);
        }

        if (filter.InStockOnly)
        {
            query = query.Where(x => x.StockQuantity > 0);
        }

        query = filter.Sort switch
        {
            ProductSorts.PriceAsc => query.OrderBy(x => x.SellPrice).ThenBy(x => x.Id),
            ProductSorts.PriceDesc => query.OrderByDescending(x => x.SellPrice).ThenBy(x => x.Id),
            ProductSorts.Name => query.OrderBy(x => x.ProductName).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.Id)
        };

        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<Product?> GetSellableByIdAsync(int id)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == ProductStatuses.Active);
    }

    public async Task<List<Category>> GetCategoriesWithSellableProductsAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(c => dbContext.Products.Any(p => p.CategoryId == c.Id && p.Status == ProductStatuses.Active))
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
    }
}
