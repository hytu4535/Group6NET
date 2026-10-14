using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

/// <summary>Giỏ hàng của người dùng đang đăng nhập. <c>userId</c> luôn lấy từ claim xác thực, không nhận từ client.</summary>
public interface IClientCartService
{
    Task<CartViewModel> GetCartAsync(int userId);
    Task<ServiceResult<CartChangeResult>> AddItemAsync(int userId, int productId, int quantity);
    Task<ServiceResult<CartChangeResult>> UpdateQuantityAsync(int userId, int itemId, int quantity);
    Task<ServiceResult<CartChangeResult>> RemoveItemAsync(int userId, int itemId);
}
