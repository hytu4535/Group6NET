using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

/// <summary>Trang giỏ hàng. Thêm/sửa/xóa dòng giỏ hàng đi qua API /api/client/cart.</summary>
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
}
