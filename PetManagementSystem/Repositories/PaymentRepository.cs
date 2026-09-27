using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class PaymentRepository(AppDbContext dbContext) : IPaymentRepository
{
    public async Task<PetManagementSystem.Helpers.PagedResult<Payment>> SearchAsync(string? search, string? status, string? method, int page, int pageSize)
    {
        var query = dbContext.Payments
            .AsNoTracking()
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var hasId = int.TryParse(keyword.TrimStart('#'), out var orderId);
            query = query.Where(x => (hasId && x.OrderId == orderId)
                                     || (x.TransactionCode != null && x.TransactionCode.Contains(keyword))
                                     || x.Order!.User!.Username.Contains(keyword)
                                     || (x.Order.User.FullName != null && x.Order.User.FullName.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(method))
        {
            query = query.Where(x => x.PaymentMethod == method);
        }

        return await query
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await dbContext.Payments
            .Include(x => x.Order)
                .ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsTransactionCodeAsync(string code, int? excludeId = null)
    {
        var query = dbContext.Payments.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(x => x.TransactionCode == code);
    }

    public async Task<decimal> GetCompletedTotalAsync(int orderId, int? excludePaymentId = null)
    {
        var query = dbContext.Payments
            .Where(x => x.OrderId == orderId && x.Status == PaymentStatuses.Completed);

        if (excludePaymentId.HasValue)
        {
            query = query.Where(x => x.Id != excludePaymentId.Value);
        }

        return await query.SumAsync(x => x.Amount);
    }

    public async Task AddAsync(Payment payment)
    {
        await dbContext.Payments.AddAsync(payment);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        // Entity đã được theo dõi thì chỉ cần SaveChanges; tránh Update() lan sang các entity liên quan.
        if (dbContext.Entry(payment).State == EntityState.Detached)
        {
            dbContext.Payments.Update(payment);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Payment payment)
    {
        dbContext.Payments.Remove(payment);
        await dbContext.SaveChangesAsync();
    }
}