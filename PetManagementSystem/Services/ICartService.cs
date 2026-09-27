using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface ICartService
{
    Task<CartListViewModel> GetListAsync(string? search, string? status, int page);
    Task<CartItemListViewModel> GetItemsAsync(int? cartId, string? search, int page);
    Task<CartDetailsViewModel?> GetDetailsAsync(int id);
    Task<ServiceResult> DeleteAsync(int id);
}
