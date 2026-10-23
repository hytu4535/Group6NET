using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers;

[Authorize]
public class CheckoutController(ICheckoutService checkoutService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var model = await checkoutService.BuildAsync(userId.Value);
        if (model.Cart.Items.Count == 0)
        {
            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
            return RedirectToAction("Index", "Cart");
        }

        if (model.Cart.HasIssues)
        {
            TempData["ErrorMessage"] = "Một số sản phẩm trong giỏ không còn đủ điều kiện đặt hàng. Vui lòng kiểm tra lại giỏ hàng.";
            return RedirectToAction("Index", "Cart");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place([Bind(Prefix = "Input")] CheckoutInputModel input)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await checkoutService.BuildAsync(userId.Value, input));
        }

        var result = await checkoutService.PlaceOrderAsync(userId.Value, input);
        if (result.Success)
        {
            TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{result.Data}.";
            return RedirectToAction("Details", "Order", new { id = result.Data });
        }

        var model = await checkoutService.BuildAsync(userId.Value, input);
        if (model.Cart.Items.Count == 0)
        {
            TempData["ErrorMessage"] = result.Error;
            return RedirectToAction("Index", "Cart");
        }

        var key = string.IsNullOrEmpty(result.Field) ? string.Empty : $"Input.{result.Field}";
        ModelState.AddModelError(key, result.Error ?? "Không thể đặt hàng.");
        return View("Index", model);
    }
}
