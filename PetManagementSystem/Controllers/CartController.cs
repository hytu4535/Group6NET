using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

/// <summary>
/// Trang giỏ hàng. Thêm/sửa/xóa sản phẩm đi qua API /api/client/cart (xem ClientCartApiController và wwwroot/js/shop.js);
/// đặt hàng thực hiện ở CheckoutController.
/// </summary>
[Authorize]
public class CartController(IClientCartService cartService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        return View(await cartService.GetCartAsync(userId.Value));
    }

    /// <summary>Giữ lại đường dẫn cũ /Cart/Checkout: chuyển sang trang thanh toán chính thức.</summary>
    [HttpGet]
    public IActionResult Checkout() => RedirectToAction("Index", "Checkout");
}
