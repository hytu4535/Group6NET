using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers
{
    public class ProductController(IStorefrontService storefrontService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] ProductCatalogQuery query)
        {
            ViewData["Title"] = "Sản phẩm thú cưng";
            var model = await storefrontService.GetProductsAsync(query);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var product = await storefrontService.GetProductAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}