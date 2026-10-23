using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class SupplierRepository(AppDbContext dbContext) : ISupplierRepository
{
    public async Task<PetManagementSystem.Models.PagedResult<Supplier>> SearchAsync(string? search, int page, int pageSize)
    {
        var query = dbContext.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.SupplierName.Contains(keyword)
                                     || x.Phone.Contains(keyword)
                                     || (x.Email != null && x.Email.Contains(keyword)));
        }

        return await query
            .OrderBy(x => x.SupplierName)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<List<Supplier>> GetAllAsync()
    {
        return await dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(x => x.SupplierName)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        return await dbContext.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
    {
        var query = dbContext.Suppliers.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.Email == email);
    }

    public async Task<bool> HasReceiptsAsync(int id)
    {
        return await dbContext.ImportReceipts.AnyAsync(x => x.SupplierId == id);
    }

    public async Task<Dictionary<int, int>> GetReceiptCountsAsync(IEnumerable<int> supplierIds)
    {
        var ids = supplierIds.ToList();
        return await dbContext.ImportReceipts
            .Where(x => ids.Contains(x.SupplierId))
            .GroupBy(x => x.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count);
    }

    public async Task AddAsync(Supplier supplier)
    {
        await dbContext.Suppliers.AddAsync(supplier);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(supplier).State == EntityState.Detached)
        {
            dbContext.Suppliers.Update(supplier);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Supplier supplier)
    {
        dbContext.Suppliers.Remove(supplier);
        await dbContext.SaveChangesAsync();
    }
}
