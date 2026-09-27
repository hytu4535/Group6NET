using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class ImportReceiptRepository(AppDbContext dbContext) : IImportReceiptRepository
{
    public async Task<PetManagementSystem.Models.PagedResult<ImportReceipt>> SearchAsync(string? search, int? supplierId, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = dbContext.ImportReceipts
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Employee)
            .Include(x => x.Details)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var hasId = int.TryParse(keyword.TrimStart('#'), out var receiptId);
            query = query.Where(x => (hasId && x.Id == receiptId)
                                     || x.Supplier!.SupplierName.Contains(keyword)
                                     || (x.Note != null && x.Note.Contains(keyword)));
        }

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(x => x.ReceiptDate >= start);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);
            query = query.Where(x => x.ReceiptDate < endExclusive);
        }

        return await query
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<PetManagementSystem.Models.PagedResult<ImportDetail>> SearchDetailsAsync(int? receiptId, string? search, int page, int pageSize)
    {
        var query = dbContext.ImportDetails
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Receipt)
                .ThenInclude(r => r!.Supplier)
            .AsQueryable();

        if (receiptId.HasValue)
        {
            query = query.Where(x => x.ReceiptId == receiptId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Product!.ProductName.Contains(keyword)
                                     || x.Receipt!.Supplier!.SupplierName.Contains(keyword));
        }

        return await query
            .OrderByDescending(x => x.ReceiptId)
            .ThenBy(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<ImportReceipt?> GetByIdWithDetailsAsync(int id)
    {
        return await dbContext.ImportReceipts
            .Include(x => x.Supplier)
            .Include(x => x.Employee)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(ImportReceipt receipt)
    {
        await dbContext.ImportReceipts.AddAsync(receipt);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(ImportReceipt receipt)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(receipt).State == EntityState.Detached)
        {
            dbContext.ImportReceipts.Update(receipt);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(ImportReceipt receipt)
    {
        dbContext.ImportReceipts.Remove(receipt);
        await dbContext.SaveChangesAsync();
    }
}