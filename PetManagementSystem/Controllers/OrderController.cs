using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers;

/// <summary>Lịch sử đơn hàng, theo dõi trạng thái, thanh toán và hóa đơn của khách hàng.</summary>
[Authorize]
public class OrderController(IClientOrderService orderService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MyOrdersQuery query)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        return View(await orderService.GetOrdersAsync(userId.Value, query));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var order = await orderService.GetOrderAsync(userId.Value, id);
        if (order is null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var result = await orderService.CancelAsync(userId.Value, id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id}.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Hóa đơn của một đơn hàng (id là mã đơn hàng).</summary>
    [HttpGet]
    public async Task<IActionResult> Invoice(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var invoice = await orderService.GetInvoiceAsync(userId.Value, id);
        if (invoice is null)
        {
            TempData["ErrorMessage"] = "Đơn hàng này chưa có hóa đơn. Hóa đơn do cửa hàng phát hành.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(invoice);
    }
}
