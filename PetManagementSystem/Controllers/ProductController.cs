using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers;

public class ProductController(IStorefrontService storefrontService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductCatalogQuery query)
    {
        return View(await storefrontService.GetProductsAsync(query));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await storefrontService.GetProductAsync(id);
        if (product is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy sản phẩm hoặc sản phẩm đã ngừng bán.";
            return RedirectToAction(nameof(Index));
        }

        return View(product);
    }
}
