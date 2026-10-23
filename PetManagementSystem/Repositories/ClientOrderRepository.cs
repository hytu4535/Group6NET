using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class ClientOrderRepository(AppDbContext dbContext) : IClientOrderRepository
{
    private sealed class EfTransaction(IDbContextTransaction inner) : IRepositoryTransaction
    {
        public Task CommitAsync() => inner.CommitAsync();
        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }

    public async Task<IRepositoryTransaction> BeginTransactionAsync()
    {
        return new EfTransaction(await dbContext.Database.BeginTransactionAsync());
    }

    public async Task<List<string>> GetRecentShippingAddressesAsync(int userId, int take)
    {
        var addresses = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId && o.ShippingAddress != "")
            .OrderByDescending(o => o.OrderDate)
            .Select(o => o.ShippingAddress)
            .Take(30)
            .ToListAsync();

        return addresses
            .Select(a => a.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .ToList();
    }

    public async Task<bool> TryReserveStockAsync(int productId, int quantity)
    {
        var now = DateTime.Now;
        var affected = await dbContext.Products
            .Where(p => p.Id == productId
                        && p.Status == ProductStatuses.Active
                        && p.StockQuantity >= quantity)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity)
                .SetProperty(p => p.UpdatedAt, (DateTime?)now));

        return affected == 1;
    }

    public async Task ReleaseStockAsync(int productId, int quantity)
    {
        var now = DateTime.Now;
        await dbContext.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.StockQuantity, p => p.StockQuantity + quantity)
                .SetProperty(p => p.UpdatedAt, (DateTime?)now));
    }

    public async Task AddOrderAsync(Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await dbContext.Payments.AddAsync(payment);
        await dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<Order>> SearchOwnedAsync(int userId, string? status, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .Where(x => x.UserId == userId);

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

    public async Task<Order?> GetOwnedDetailsAsync(int userId, int orderId)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Include(x => x.Payments)
            .Include(x => x.Invoice)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId);
    }

    public async Task<Invoice?> GetOwnedInvoiceAsync(int userId, int orderId)
    {
        return await dbContext.Invoices
            .AsNoTracking()
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .Include(x => x.Order)
                .ThenInclude(o => o!.Items)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.OrderId == orderId && x.Order!.UserId == userId);
    }

    public async Task<bool> TryCancelPendingAsync(int userId, int orderId)
    {
        var affected = await dbContext.Orders
            .Where(o => o.Id == orderId && o.UserId == userId && o.Status == OrderStatuses.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatuses.Cancelled));

        return affected == 1;
    }

    public async Task<bool> HasCompletedPaymentAsync(int orderId)
    {
        return await dbContext.Payments
            .AnyAsync(p => p.OrderId == orderId && p.Status == PaymentStatuses.Completed);
    }

    public async Task FailPendingPaymentsAsync(int orderId)
    {
        await dbContext.Payments
            .Where(p => p.OrderId == orderId && p.Status == PaymentStatuses.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PaymentStatuses.Failed));
    }
}
