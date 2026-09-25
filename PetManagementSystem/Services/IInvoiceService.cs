using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface IInvoiceService
{
    Task<InvoiceListViewModel> GetListAsync(string? search, DateTime? from, DateTime? to, int page);
    Task<InvoiceDetailsViewModel?> GetDetailsAsync(int id);

    /// <summary>Tạo (hoặc nạp lại) model form lập hóa đơn; <paramref name="orderId"/> để chọn sẵn đơn hàng.</summary>
    Task<InvoiceUpsertViewModel> BuildCreateModelAsync(int? orderId = null, InvoiceUpsertViewModel? model = null);

    Task<InvoiceUpsertViewModel?> GetForEditAsync(int id);
    Task<ServiceResult<int>> CreateAsync(InvoiceUpsertViewModel model, int? issuedBy);
    Task<ServiceResult> UpdateAsync(InvoiceUpsertViewModel model);
}
