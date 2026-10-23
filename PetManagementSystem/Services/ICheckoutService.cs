using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public interface ICheckoutService
{
    /// <summary>Dựng model trang thanh toán; <c>input</c> null thì điền sẵn từ hồ sơ người dùng.</summary>
    Task<CheckoutViewModel> BuildAsync(int userId, CheckoutInputModel? input = null);

    /// <summary>Đặt hàng từ giỏ đang hoạt động. Trả về mã đơn hàng.</summary>
    Task<ServiceResult<int>> PlaceOrderAsync(int userId, CheckoutInputModel input);
}
