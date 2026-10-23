using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public interface IImportReceiptService
{
    Task<ImportReceiptListViewModel> GetListAsync(string? search, int? supplierId, DateTime? from, DateTime? to, int page);
    Task<ImportDetailListViewModel> GetDetailLinesAsync(int? receiptId, string? search, int page);

    /// <summary>Tạo (hoặc nạp lại) model form nhập kho kèm danh sách nhà cung cấp và sản phẩm.</summary>
    Task<ImportReceiptCreateViewModel> BuildCreateModelAsync(ImportReceiptCreateViewModel? model = null);

    Task<ServiceResult<int>> CreateAsync(ImportReceiptCreateViewModel model, int? employeeId);
    Task<ImportReceiptDetailsViewModel?> GetDetailsAsync(int id);
    Task<ImportReceiptEditViewModel?> GetForEditAsync(int id);
    Task PopulateOptionsAsync(ImportReceiptEditViewModel model);
    Task<ServiceResult> UpdateAsync(ImportReceiptEditViewModel model);

    /// <summary>Xóa phiếu nhập và hoàn tác tồn kho tương ứng.</summary>
    Task<ServiceResult> DeleteAsync(int id);
}
