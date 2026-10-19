using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers.Api;

[AllowAnonymous]
[Route("api/client/products")]
public class ClientProductsApiController(IStorefrontService storefrontService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductCardViewModel>>>> GetProducts(
        [FromQuery] ProductCatalogQuery query)
    {
        if (!TryValidateQuery(query, out var errors))
        {
            return BadRequest(ApiResponse<IReadOnlyList<ProductCardViewModel>>.Fail(
                "Tham số tìm kiếm sản phẩm không hợp lệ.", [.. errors]));
        }

        var result = await storefrontService.GetProductsAsync(query);
        return Ok(ApiResponse<IReadOnlyList<ProductCardViewModel>>.Ok(
            result.Items,
            "Lấy danh sách sản phẩm thành công.",
            result.Page,
            result.PageSize,
            result.TotalItems));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryFilterViewModel>>>> GetCategories()
    {
        var categories = await storefrontService.GetProductCategoriesAsync();
        return Ok(ApiResponse<IReadOnlyList<CategoryFilterViewModel>>.Ok(
            categories, "Lấy danh sách danh mục sản phẩm thành công."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDetailViewModel>>> GetProduct(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponse<ProductDetailViewModel>.Fail(
                "Mã sản phẩm phải lớn hơn 0."));
        }

        var product = await storefrontService.GetProductAsync(id);
        return product is null
            ? NotFound(ApiResponse<ProductDetailViewModel>.Fail(
                "Không tìm thấy sản phẩm hoặc sản phẩm đã ngừng bán."))
            : Ok(ApiResponse<ProductDetailViewModel>.Ok(
                product, "Lấy chi tiết sản phẩm thành công."));
    }

    private bool TryValidateQuery(ProductCatalogQuery query, out List<string> errors)
    {
        errors = [];
        if (!ModelState.IsValid)
        {
            errors.AddRange(ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Giá trị tham số không hợp lệ."
                    : error.ErrorMessage));
        }
        if (query.Page is < 1 or > 1_000_000)
        {
            errors.Add("page phải trong khoảng từ 1 đến 1.000.000.");
        }
        if (query.PageSize is < 1 or > 100)
        {
            errors.Add("pageSize phải trong khoảng từ 1 đến 100.");
        }
        if (query.CategoryId is <= 0)
        {
            errors.Add("categoryId phải lớn hơn 0.");
        }
        if (query.Search?.Length > 100)
        {
            errors.Add("search không được vượt quá 100 ký tự.");
        }
        if (query.MinPrice is < 0 || query.MaxPrice is < 0)
        {
            errors.Add("Giá sản phẩm không được là số âm.");
        }
        if (!string.IsNullOrWhiteSpace(query.Sort) &&
            !ProductSorts.All.Contains(query.Sort.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            errors.Add("sort không được hỗ trợ.");
        }

        return errors.Count == 0;
    }
}
