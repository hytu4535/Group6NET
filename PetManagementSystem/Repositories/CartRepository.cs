using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class CartRepository(AppDbContext dbContext) : ICartRepository
{
    public async Task<PetManagementSystem.Models.PagedResult<Cart>> SearchAsync(string? search, string? status, int page, int pageSize)
    {
        var query = dbContext.Carts
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var hasId = int.TryParse(keyword.TrimStart('#'), out var cartId);
            query = query.Where(x => (hasId && x.Id == cartId)
                                     || x.User!.Username.Contains(keyword)
                                     || (x.User.FullName != null && x.User.FullName.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    /// <summary>Phân trang theo giỏ hàng (không theo dòng sản phẩm) để mỗi giỏ luôn hiển thị đầy đủ trong một khối.</summary>
    public async Task<PetManagementSystem.Models.PagedResult<Cart>> SearchWithItemsAsync(int? cartId, string? search, int page, int pageSize)
    {
        var query = dbContext.Carts
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .Where(x => x.Items.Any())
            .AsQueryable();

        if (cartId.HasValue)
        {
            query = query.Where(x => x.Id == cartId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(x => x.User!.Username.Contains(keyword)
                                     || (x.User.FullName != null && x.User.FullName.Contains(keyword))
                                     || x.Items.Any(i => i.Product!.ProductName.Contains(keyword)));
        }

        return await query
            .OrderByDescending(x => x.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public async Task<Cart?> GetByIdWithItemsAsync(int id)
    {
        return await dbContext.Carts
            .Include(x => x.User)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task DeleteAsync(Cart cart)
    {
        dbContext.Carts.Remove(cart);
        await dbContext.SaveChangesAsync();
    }
}