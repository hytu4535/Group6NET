using Microsoft.AspNetCore.Mvc.Rendering;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository) : IProductService
{
    public async Task<ProductListViewModel> GetListAsync(string? search, int? categoryId, string? status, int page)
    {
        var normalizedStatus = CommerceMapping.NormalizeOption(status, ProductStatuses.All);
        var result = await productRepository.SearchAsync(
            search,
            categoryId,
            string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            page,
            Paging.DefaultPageSize);

        return new ProductListViewModel
        {
            Search = search?.Trim(),
            CategoryId = categoryId,
            Status = string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            CategoryOptions = await GetCategoryOptionsAsync(),
            Items = result.Items.Select(x => new ProductRowViewModel
            {
                Id = x.Id,
                ProductName = x.ProductName,
                CategoryName = x.Category?.CategoryName ?? string.Empty,
                Unit = x.Unit,
                ImportPrice = x.ImportPrice,
                SellPrice = x.SellPrice,
                StockQuantity = x.StockQuantity,
                ImageUrl = x.ImageUrl,
                Status = x.Status
            }).ToList()
        };
    }

    public async Task<ProductUpsertViewModel> BuildCreateModelAsync()
    {
        var model = new ProductUpsertViewModel();
        await PopulateOptionsAsync(model);
        return model;
    }

    public async Task<ProductUpsertViewModel?> GetForEditAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return null;
        }

        var model = new ProductUpsertViewModel
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            ProductName = product.ProductName,
            Description = product.Description,
            Unit = product.Unit,
            ImportPrice = product.ImportPrice,
            SellPrice = product.SellPrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            Status = product.Status
        };

        await PopulateOptionsAsync(model);
        return model;
    }

    public async Task PopulateOptionsAsync(ProductUpsertViewModel model)
    {
        model.CategoryOptions = await GetCategoryOptionsAsync();
    }

    public async Task<ServiceResult> CreateAsync(ProductUpsertViewModel model)
    {
        var check = await ValidateAsync(model, excludeId: null);
        if (!check.Success)
        {
            return check;
        }

        var product = new Product { CreatedAt = DateTime.Now };
        Apply(product, model);

        await productRepository.AddAsync(product);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(ProductUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được sản phẩm cần cập nhật");
        }

        var product = await productRepository.GetByIdAsync(model.Id.Value);
        if (product is null)
        {
            return ServiceResult.Fail("Không tìm thấy sản phẩm");
        }

        var check = await ValidateAsync(model, product.Id);
        if (!check.Success)
        {
            return check;
        }

        Apply(product, model);
        product.UpdatedAt = DateTime.Now;

        await productRepository.UpdateAsync(product);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return ServiceResult.Fail("Không tìm thấy sản phẩm");
        }

        product.Status = product.Status == ProductStatuses.Active
            ? ProductStatuses.Inactive
            : ProductStatuses.Active;
        product.UpdatedAt = DateTime.Now;

        await productRepository.UpdateAsync(product);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product is null)
        {
            return ServiceResult.Fail("Không tìm thấy sản phẩm");
        }

        if (await productRepository.IsReferencedAsync(id))
        {
            return ServiceResult.Fail("Sản phẩm đã phát sinh phiếu nhập, giỏ hàng hoặc đơn hàng nên không thể xóa. Hãy chuyển sang \"Ngừng bán\".");
        }

        await productRepository.DeleteAsync(product);
        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> ValidateAsync(ProductUpsertViewModel model, int? excludeId)
    {
        if (await categoryRepository.GetByIdAsync(model.CategoryId) is null)
        {
            return ServiceResult.Fail("Danh mục không tồn tại", nameof(model.CategoryId));
        }

        if (await productRepository.ExistsNameAsync(model.ProductName.Trim(), model.CategoryId, excludeId))
        {
            return ServiceResult.Fail("Đã có sản phẩm cùng tên trong danh mục này", nameof(model.ProductName));
        }

        return ServiceResult.Ok();
    }

    private static void Apply(Product product, ProductUpsertViewModel model)
    {
        product.CategoryId = model.CategoryId;
        product.ProductName = model.ProductName.Trim();
        product.Description = model.Description.TrimToNull();
        product.Unit = model.Unit.TrimToNull();
        product.ImportPrice = Math.Round(model.ImportPrice, 2);
        product.SellPrice = Math.Round(model.SellPrice, 2);
        product.StockQuantity = model.StockQuantity;
        product.ImageUrl = model.ImageUrl.TrimToNull();
        product.Status = model.Status;
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoryOptionsAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.CategoryName
        }).ToList();
    }
}
