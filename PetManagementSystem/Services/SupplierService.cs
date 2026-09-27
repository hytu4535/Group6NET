using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class SupplierService(ISupplierRepository supplierRepository) : ISupplierService
{
    public async Task<SupplierListViewModel> GetListAsync(string? search, int page)
    {
        var result = await supplierRepository.SearchAsync(search, page, Paging.DefaultPageSize);
        var counts = await supplierRepository.GetReceiptCountsAsync(result.Items.Select(x => x.Id));

        return new SupplierListViewModel
        {
            Search = search?.Trim(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new SupplierRowViewModel
            {
                Id = x.Id,
                SupplierName = x.SupplierName,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address,
                ReceiptCount = counts.GetValueOrDefault(x.Id),
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }

    public async Task<SupplierUpsertViewModel?> GetForEditAsync(int id)
    {
        var supplier = await supplierRepository.GetByIdAsync(id);
        if (supplier is null)
        {
            return null;
        }

        return new SupplierUpsertViewModel
        {
            Id = supplier.Id,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address
        };
    }

    public async Task<ServiceResult> CreateAsync(SupplierUpsertViewModel model)
    {
        var email = model.Email!.Trim();
        if (await supplierRepository.ExistsEmailAsync(email))
        {
            return ServiceResult.Fail("Email đã được dùng bởi nhà cung cấp khác", nameof(model.Email));
        }

        await supplierRepository.AddAsync(new Supplier
        {
            SupplierName = model.SupplierName.Trim(),
            Phone = model.Phone.Trim(),
            Email = email,
            Address = model.Address.TrimToNull(),
            CreatedAt = DateTime.Now
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(SupplierUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được nhà cung cấp cần cập nhật");
        }

        var supplier = await supplierRepository.GetByIdAsync(model.Id.Value);
        if (supplier is null)
        {
            return ServiceResult.Fail("Không tìm thấy nhà cung cấp");
        }

        var email = model.Email!.Trim();
        if (await supplierRepository.ExistsEmailAsync(email, supplier.Id))
        {
            return ServiceResult.Fail("Email đã được dùng bởi nhà cung cấp khác", nameof(model.Email));
        }

        supplier.SupplierName = model.SupplierName.Trim();
        supplier.Phone = model.Phone.Trim();
        supplier.Email = email;
        supplier.Address = model.Address.TrimToNull();

        await supplierRepository.UpdateAsync(supplier);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var supplier = await supplierRepository.GetByIdAsync(id);
        if (supplier is null)
        {
            return ServiceResult.Fail("Không tìm thấy nhà cung cấp");
        }

        if (await supplierRepository.HasReceiptsAsync(id))
        {
            return ServiceResult.Fail("Không thể xóa nhà cung cấp đã có phiếu nhập kho");
        }

        await supplierRepository.DeleteAsync(supplier);
        return ServiceResult.Ok();
    }
}
