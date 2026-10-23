using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public class StorefrontService(IStorefrontRepository storefrontRepository) : IStorefrontService
{
    private const int DefaultProductPageSize = 12;

    public async Task<ProductListViewModel> GetProductsAsync(ProductCatalogQuery query)
    {
        query.Page = Math.Clamp(query.Page, 1, 1_000_000);
        query.PageSize = query.PageSize <= 0
            ? DefaultProductPageSize
            : Math.Clamp(query.PageSize, 1, 100);
        var sort = CommerceMapping.NormalizeOption(query.Sort, ProductSorts.All);
        if (sort.Length == 0)
        {
            sort = ProductSorts.Newest;
        }

        var minPrice = query.MinPrice is >= 0 ? query.MinPrice : null;
        var maxPrice = query.MaxPrice is >= 0 ? query.MaxPrice : null;
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            (minPrice, maxPrice) = (maxPrice, minPrice);
        }

        var filter = new ProductFilter(
            query.Search.TrimToNull(),
            query.CategoryId,
            minPrice,
            maxPrice,
            query.InStock,
            sort);

        var result = await storefrontRepository.SearchAsync(filter, query.Page, query.PageSize);
        var categories = await storefrontRepository.GetCategoriesWithSellableProductsAsync();

        return new ProductListViewModel
        {
            Search = filter.Search,
            CategoryId = filter.CategoryId,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            InStock = filter.InStockOnly,
            Sort = sort,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Categories = categories.Select(c => new CategoryFilterViewModel { Id = c.Id, Name = c.CategoryName }).ToList(),
            Items = result.Items.Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.ProductName,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Unit = p.Unit,
                CategoryName = p.Category?.CategoryName,
                Price = p.SellPrice,
                StockQuantity = p.StockQuantity
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<CategoryFilterViewModel>> GetProductCategoriesAsync()
    {
        var categories = await storefrontRepository.GetCategoriesWithSellableProductsAsync();
        return categories
            .Select(category => new CategoryFilterViewModel
            {
                Id = category.Id,
                Name = category.CategoryName
            })
            .ToList();
    }

    public async Task<ProductDetailViewModel?> GetProductAsync(int id)
    {
        var product = await storefrontRepository.GetSellableByIdAsync(id);
        if (product is null)
        {
            return null;
        }

        return new ProductDetailViewModel
        {
            Id = product.Id,
            Name = product.ProductName,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Unit = product.Unit,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.CategoryName,
            Price = product.SellPrice,
            StockQuantity = product.StockQuantity
        };
    }
}
