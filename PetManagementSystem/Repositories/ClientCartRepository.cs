using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class ClientCartRepository(AppDbContext dbContext) : IClientCartRepository
{
    public async Task<Cart?> GetActiveCartAsync(int userId, bool includeItems)
    {
        IQueryable<Cart> query = dbContext.Carts;
        if (includeItems)
        {
            query = query.Include(c => c.Items).ThenInclude(i => i.Product);
        }

        return await query
            .Where(c => c.UserId == userId && c.Status == CartStatuses.Active)
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Cart> CreateActiveCartAsync(int userId)
    {
        var cart = new Cart
        {
            UserId = userId,
            Status = CartStatuses.Active,
            CreatedAt = DateTime.Now
        };

        await dbContext.Carts.AddAsync(cart);
        await dbContext.SaveChangesAsync();
        return cart;
    }

    public async Task<CartItem?> GetActiveItemAsync(int userId, int itemId)
    {
        return await dbContext.CartItems
            .Include(i => i.Cart)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId
                                      && i.Cart!.UserId == userId
                                      && i.Cart.Status == CartStatuses.Active);
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        return await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
    }

    public void AddItem(CartItem item) => dbContext.CartItems.Add(item);

    public void RemoveItem(CartItem item) => dbContext.CartItems.Remove(item);

    public async Task<bool> TrySaveAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> TryMarkCheckedOutAsync(int cartId, int userId)
    {
        var now = DateTime.Now;
        var affected = await dbContext.Carts
            .Where(c => c.Id == cartId && c.UserId == userId && c.Status == CartStatuses.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, CartStatuses.CheckedOut)
                .SetProperty(c => c.UpdatedAt, (DateTime?)now));

        return affected == 1;
    }
}
