using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class InvoiceRepository(AppDbContext dbContext) : IInvoiceRepository
{
    public async Task<PetManagementSystem.Helpers.PagedResult<Invoice>> SearchAsync(string? search, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .Include(x => x.IssuedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var hasId = int.TryParse(keyword.TrimStart('#'), out var number);
            query = query.Where(x => (hasId && (x.Id == number || x.OrderId == number))
                                     || x.Order!.User!.Username.Contains(keyword)
                                     || (x.Order.User.FullName != null && x.Order.User.FullName.Contains(keyword)));
        }

        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(x => x.InvoiceDate >= start);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);
            query = query.Where(x => x.InvoiceDate < endExclusive);
        }

        return await query
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await dbContext.Invoices
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .Include(x => x.Order)
                .ThenInclude(o => o!.Items)
                    .ThenInclude(i => i.Product)
            .Include(x => x.IssuedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsForOrderAsync(int orderId, int? excludeInvoiceId = null)
    {
        var query = dbContext.Invoices.AsQueryable();
        if (excludeInvoiceId.HasValue)
        {
            query = query.Where(x => x.Id != excludeInvoiceId.Value);
        }

        return await query.AnyAsync(x => x.OrderId == orderId);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await dbContext.Invoices.AddAsync(invoice);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(invoice).State == EntityState.Detached)
        {
            dbContext.Invoices.Update(invoice);
        }
        await dbContext.SaveChangesAsync();
    }
}