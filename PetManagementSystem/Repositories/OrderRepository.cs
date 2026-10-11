using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class OrderRepository(AppDbContext dbContext) : IOrderRepository
{
    private const int SelectionLimit = 500;

    public async Task<PetManagementSystem.Models.PagedResult<Order>> SearchAsync(string? search, string? status, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var hasId = int.TryParse(keyword.TrimStart('#'), out var orderId);
            query = query.Where(x => (hasId && x.Id == orderId)
                                     || x.Phone.Contains(keyword)
                                     || x.User!.Username.Contains(keyword)
                                     || (x.User.FullName != null && x.User.FullName.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(x => x.OrderDate >= start);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);
            query = query.Where(x => x.OrderDate < endExclusive);
        }

        return await query
            .OrderByDescending(x => x.OrderDate)
            .ThenByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<PetManagementSystem.Models.PagedResult<OrderItem>> SearchItemsAsync(int? orderId, string? search, int page, int pageSize)
    {
        var query = dbContext.OrderItems
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(x => x.OrderId == orderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.Product!.ProductName.Contains(keyword)
                                     || x.Order!.User!.Username.Contains(keyword)
                                     || (x.Order.User.FullName != null && x.Order.User.FullName.Contains(keyword)));
        }

        return await query
            .OrderByDescending(x => x.OrderId)
            .ThenBy(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        return await dbContext.Orders
            .Include(x => x.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.Payments)
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Order>> GetOrdersForPaymentAsync()
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Payments)
            .Where(x => x.Status != OrderStatuses.Cancelled)
            .OrderByDescending(x => x.OrderDate)
            .ThenByDescending(x => x.Id)
            .Take(SelectionLimit)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersForInvoiceAsync(int? includeOrderId = null)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Payments)
            .Where(x => (x.Status != OrderStatuses.Cancelled && x.Invoice == null)
                        || (includeOrderId.HasValue && x.Id == includeOrderId.Value))
            .OrderByDescending(x => x.OrderDate)
            .ThenByDescending(x => x.Id)
            .Take(SelectionLimit)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(order).State == EntityState.Detached)
        {
            dbContext.Orders.Update(order);
        }
        await dbContext.SaveChangesAsync();
    }
}
