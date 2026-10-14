using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

/// <summary>Giỏ hàng của khách hàng. Mọi truy vấn đều ràng buộc theo chủ sở hữu (user_id).</summary>
public interface IClientCartRepository
{
    /// <summary>Giỏ đang hoạt động của người dùng (có theo dõi thay đổi), null nếu chưa có.</summary>
    Task<Cart?> GetActiveCartAsync(int userId, bool includeItems);

    Task<Cart> CreateActiveCartAsync(int userId);

    /// <summary>Dòng giỏ hàng thuộc giỏ đang hoạt động của chính người dùng; ngược lại trả về null.</summary>
    Task<CartItem?> GetActiveItemAsync(int userId, int itemId);

    Task<Product?> GetProductAsync(int productId);

    void AddItem(CartItem item);
    void RemoveItem(CartItem item);

    /// <summary>Lưu thay đổi; trả về false khi vi phạm ràng buộc (ví dụ hai yêu cầu thêm cùng sản phẩm đồng thời).</summary>
    Task<bool> TrySaveAsync();

    /// <summary>Chuyển giỏ từ active sang checked_out một cách nguyên tử. False nếu giỏ đã được xử lý.</summary>
    Task<bool> TryMarkCheckedOutAsync(int cartId, int userId);
}
