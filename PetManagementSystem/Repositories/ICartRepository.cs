using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface ICartRepository
{
    Task<PagedResult<Cart>> SearchAsync(string? search, string? status, int page, int pageSize);
    Task<PagedResult<CartItem>> SearchItemsAsync(int? cartId, string? search, int page, int pageSize);
    Task<Cart?> GetByIdWithItemsAsync(int id);
    Task DeleteAsync(Cart cart);
}
