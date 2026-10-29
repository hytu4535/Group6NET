using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Services;

namespace PetManagementSystem.ViewComponents;

/// <summary>Icon giỏ hàng trên thanh điều hướng, kèm tổng số lượng sản phẩm trong giỏ đang hoạt động của người dùng.</summary>
public class CartBadgeViewComponent(IClientCartService cartService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.GetUserId();
        var quantity = userId is null ? 0 : (await cartService.GetCartAsync(userId.Value)).TotalQuantity;
        return View(quantity);
    }
}
