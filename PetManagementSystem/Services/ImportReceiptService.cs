using Microsoft.AspNetCore.Mvc.Rendering;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class ImportReceiptService(
    IImportReceiptRepository receiptRepository,
    ISupplierRepository supplierRepository,
    IProductRepository productRepository) : IImportReceiptService
{
    public async Task<ImportReceiptListViewModel> GetListAsync(string? search, int? supplierId, DateTime? from, DateTime? to, int page)
    {
        var result = await receiptRepository.SearchAsync(search, supplierId, from, to, page, Paging.DefaultPageSize);

        return new ImportReceiptListViewModel
        {
            Search = search?.Trim(),
            SupplierId = supplierId,
            From = from,
            To = to,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            SupplierOptions = await GetSupplierOptionsAsync(),
            Items = result.Items.Select(x => new ImportReceiptRowViewModel
            {
                Id = x.Id,
                SupplierName = x.Supplier?.SupplierName ?? string.Empty,
                EmployeeName = x.Employee is null ? "-" : CommerceFormat.DisplayName(x.Employee),
                ReceiptDate = x.ReceiptDate,
                TotalAmount = x.TotalAmount,
                LineCount = x.Details.Count,
                Note = x.Note
            }).ToList()
        };
    }

    public async Task<ImportDetailListViewModel> GetDetailLinesAsync(int? receiptId, string? search, int page)
    {
        var result = await receiptRepository.SearchDetailsAsync(receiptId, search, page, Paging.DefaultPageSize);

        return new ImportDetailListViewModel
        {
            ReceiptId = receiptId,
            Search = search?.Trim(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new ImportDetailRowViewModel
            {
                Id = x.Id,
                ReceiptId = x.ReceiptId,
                SupplierName = x.Receipt?.Supplier?.SupplierName ?? string.Empty,
                ReceiptDate = x.Receipt?.ReceiptDate ?? default,
                ProductName = x.Product?.ProductName ?? $"#{x.ProductId}",
                Unit = x.Product?.Unit,
                Quantity = x.Quantity,
                ImportPrice = x.ImportPrice,
                Subtotal = x.Subtotal
            }).ToList()
        };
    }

    public async Task<ImportReceiptCreateViewModel> BuildCreateModelAsync(ImportReceiptCreateViewModel? model = null)
    {
        model ??= new ImportReceiptCreateViewModel();
        model.SupplierOptions = await GetSupplierOptionsAsync();

        var products = await productRepository.GetOptionsAsync(onlyActive: false);
        model.ProductOptions = products.Select(x => x.ToOption()).ToList();
        return model;
    }

    public async Task<ServiceResult<int>> CreateAsync(ImportReceiptCreateViewModel model, int? employeeId)
    {
        if (model.Lines.Count == 0)
        {
            return ServiceResult<int>.Fail("Phiếu nhập cần có ít nhất một sản phẩm");
        }

        if (await supplierRepository.GetByIdAsync(model.SupplierId) is null)
        {
            return ServiceResult<int>.Fail("Nhà cung cấp không tồn tại", nameof(model.SupplierId));
        }

        if (model.Lines.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
        {
            return ServiceResult<int>.Fail("Mỗi sản phẩm chỉ được xuất hiện một lần trong phiếu nhập. Hãy gộp số lượng vào cùng một dòng.");
        }

        var products = (await productRepository.GetByIdsAsync(model.Lines.Select(x => x.ProductId)))
            .ToDictionary(x => x.Id);

        var receipt = new ImportReceipt
        {
            SupplierId = model.SupplierId,
            EmployeeId = employeeId,
            ReceiptDate = model.ReceiptDate,
            Note = model.Note.TrimToNull()
        };

        foreach (var line in model.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return ServiceResult<int>.Fail("Có sản phẩm trong phiếu không còn tồn tại");
            }

            if ((long)product.StockQuantity + line.Quantity > int.MaxValue)
            {
                return ServiceResult<int>.Fail($"Tồn kho của \"{product.ProductName}\" vượt quá giới hạn cho phép");
            }

            var price = Math.Round(line.ImportPrice, 2);
            receipt.Details.Add(new ImportDetail
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                ImportPrice = price,
                Subtotal = price * line.Quantity
            });
        }

        receipt.TotalAmount = receipt.Details.Sum(x => x.Subtotal);
        if (receipt.TotalAmount > CommerceMapping.MaxMoney)
        {
            return ServiceResult<int>.Fail("Tổng giá trị phiếu nhập vượt quá giới hạn cho phép");
        }

        // Tăng tồn kho; các thay đổi này được lưu cùng một lần SaveChanges với phiếu nhập.
        foreach (var line in model.Lines)
        {
            var product = products[line.ProductId];
            product.StockQuantity += line.Quantity;
            product.UpdatedAt = DateTime.Now;
        }

        await receiptRepository.AddAsync(receipt);
        return ServiceResult<int>.Ok(receipt.Id);
    }

    public async Task<ImportReceiptDetailsViewModel?> GetDetailsAsync(int id)
    {
        var receipt = await receiptRepository.GetByIdWithDetailsAsync(id);
        if (receipt is null)
        {
            return null;
        }

        return new ImportReceiptDetailsViewModel
        {
            Id = receipt.Id,
            SupplierName = receipt.Supplier?.SupplierName ?? string.Empty,
            EmployeeName = receipt.Employee is null ? "-" : CommerceFormat.DisplayName(receipt.Employee),
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            Note = receipt.Note,
            Lines = receipt.Details.OrderBy(x => x.Id).Select(x => new ImportReceiptLineRowViewModel
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product?.ProductName ?? $"#{x.ProductId}",
                Unit = x.Product?.Unit,
                Quantity = x.Quantity,
                ImportPrice = x.ImportPrice,
                Subtotal = x.Subtotal
            }).ToList()
        };
    }

    public async Task<ImportReceiptEditViewModel?> GetForEditAsync(int id)
    {
        var receipt = await receiptRepository.GetByIdWithDetailsAsync(id);
        if (receipt is null)
        {
            return null;
        }

        var model = new ImportReceiptEditViewModel
        {
            Id = receipt.Id,
            SupplierId = receipt.SupplierId,
            ReceiptDate = receipt.ReceiptDate,
            Note = receipt.Note
        };

        await PopulateOptionsAsync(model);
        return model;
    }

    public async Task PopulateOptionsAsync(ImportReceiptEditViewModel model)
    {
        model.SupplierOptions = await GetSupplierOptionsAsync();
    }

    public async Task<ServiceResult> UpdateAsync(ImportReceiptEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được phiếu nhập cần cập nhật");
        }

        var receipt = await receiptRepository.GetByIdWithDetailsAsync(model.Id.Value);
        if (receipt is null)
        {
            return ServiceResult.Fail("Không tìm thấy phiếu nhập");
        }

        if (await supplierRepository.GetByIdAsync(model.SupplierId) is null)
        {
            return ServiceResult.Fail("Nhà cung cấp không tồn tại", nameof(model.SupplierId));
        }

        // Chỉ cho sửa thông tin chung; dòng hàng/số lượng ảnh hưởng tồn kho nên không sửa trực tiếp.
        receipt.SupplierId = model.SupplierId;
        receipt.ReceiptDate = model.ReceiptDate;
        receipt.Note = model.Note.TrimToNull();

        await receiptRepository.UpdateAsync(receipt);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var receipt = await receiptRepository.GetByIdWithDetailsAsync(id);
        if (receipt is null)
        {
            return ServiceResult.Fail("Không tìm thấy phiếu nhập");
        }

        var products = (await productRepository.GetByIdsAsync(receipt.Details.Select(x => x.ProductId)))
            .ToDictionary(x => x.Id);

        foreach (var detail in receipt.Details)
        {
            if (products.TryGetValue(detail.ProductId, out var product) && product.StockQuantity < detail.Quantity)
            {
                return ServiceResult.Fail(
                    $"Không thể xóa: tồn kho hiện tại của \"{product.ProductName}\" ({product.StockQuantity}) " +
                    $"thấp hơn số lượng đã nhập ({detail.Quantity}).");
            }
        }

        foreach (var detail in receipt.Details)
        {
            if (products.TryGetValue(detail.ProductId, out var product))
            {
                product.StockQuantity -= detail.Quantity;
                product.UpdatedAt = DateTime.Now;
            }
        }

        await receiptRepository.DeleteAsync(receipt);
        return ServiceResult.Ok();
    }

    private async Task<IEnumerable<SelectListItem>> GetSupplierOptionsAsync()
    {
        var suppliers = await supplierRepository.GetAllAsync();
        return suppliers.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.SupplierName
        }).ToList();
    }
}
