using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface IOrderService
{
    Task<OrderListViewModel> GetListAsync(string? search, string? status, DateTime? from, DateTime? to, int page);
    Task<OrderItemListViewModel> GetItemsAsync(int? orderId, string? search, int page);
    Task<OrderDetailsViewModel?> GetDetailsAsync(int id);

    /// <summary>Tạo (hoặc nạp lại) model form tạo đơn kèm danh sách khách hàng và sản phẩm còn hàng.</summary>
    Task<OrderCreateViewModel> BuildCreateModelAsync(OrderCreateViewModel? model = null);

    /// <summary>Tạo đơn (trạng thái chờ xác nhận) và trừ tồn kho.</summary>
    Task<ServiceResult<int>> CreateAsync(OrderCreateViewModel model);

    Task<OrderEditViewModel?> GetForEditAsync(int id);
    Task<ServiceResult> UpdateAsync(OrderEditViewModel model);

    /// <summary>Chuyển trạng thái theo quy trình; hủy đơn sẽ hoàn lại tồn kho.</summary>
    Task<ServiceResult> ChangeStatusAsync(int id, string newStatus);
}
