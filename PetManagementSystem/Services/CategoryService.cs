using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryListViewModel> GetListAsync(string? search, int page)
    {
        var result = await categoryRepository.SearchAsync(search, page, Paging.DefaultPageSize);
        var counts = await categoryRepository.GetProductCountsAsync(result.Items.Select(x => x.Id));

        return new CategoryListViewModel
        {
            Search = search?.Trim(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new CategoryRowViewModel
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                Description = x.Description,
                ProductCount = counts.GetValueOrDefault(x.Id),
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }

    public async Task<CategoryUpsertViewModel?> GetForEditAsync(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return null;
        }

        return new CategoryUpsertViewModel
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };
    }

    public async Task<ServiceResult> CreateAsync(CategoryUpsertViewModel model)
    {
        var name = model.CategoryName.Trim();
        if (await categoryRepository.ExistsNameAsync(name))
        {
            return ServiceResult.Fail("Tên danh mục đã tồn tại", nameof(model.CategoryName));
        }

        await categoryRepository.AddAsync(new Category
        {
            CategoryName = name,
            Description = model.Description.TrimToNull(),
            CreatedAt = DateTime.Now
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(CategoryUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được danh mục cần cập nhật");
        }

        var category = await categoryRepository.GetByIdAsync(model.Id.Value);
        if (category is null)
        {
            return ServiceResult.Fail("Không tìm thấy danh mục");
        }

        var name = model.CategoryName.Trim();
        if (await categoryRepository.ExistsNameAsync(name, category.Id))
        {
            return ServiceResult.Fail("Tên danh mục đã tồn tại", nameof(model.CategoryName));
        }

        category.CategoryName = name;
        category.Description = model.Description.TrimToNull();

        await categoryRepository.UpdateAsync(category);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category is null)
        {
            return ServiceResult.Fail("Không tìm thấy danh mục");
        }

        if (await categoryRepository.HasProductsAsync(id))
        {
            return ServiceResult.Fail("Không thể xóa danh mục đang có sản phẩm. Hãy chuyển sản phẩm sang danh mục khác trước.");
        }

        await categoryRepository.DeleteAsync(category);
        return ServiceResult.Ok();
    }
}
