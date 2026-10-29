using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface IPaymentService
{
    Task<PaymentListViewModel> GetListAsync(string? search, string? status, string? method, int page);

    /// <summary>Tạo (hoặc nạp lại) model form tạo thanh toán; <paramref name="orderId"/> để chọn sẵn đơn hàng.</summary>
    Task<PaymentUpsertViewModel> BuildCreateModelAsync(int? orderId = null, PaymentUpsertViewModel? model = null);

    Task<PaymentUpsertViewModel?> GetForEditAsync(int id);
    Task<ServiceResult> CreateAsync(PaymentUpsertViewModel model);
    Task<ServiceResult> UpdateAsync(PaymentUpsertViewModel model);

    /// <summary>Chỉ xóa được giao dịch đang chờ hoặc thất bại.</summary>
    Task<ServiceResult> DeleteAsync(int id);
}
