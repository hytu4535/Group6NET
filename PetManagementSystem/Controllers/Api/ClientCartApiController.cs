using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Controllers.Api;

/// <summary>
/// API giỏ hàng cho giao diện Client. Không dùng [ApiController] để mọi lỗi validate đều trả về
/// đúng cấu trúc ApiResponse { success, message, data, errors }.
/// </summary>
[Authorize]
[AutoValidateAntiforgeryToken]
[Route("api/client/cart")]
public class ClientCartApiController(IClientCartService cartService) : ControllerBase
{
    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartChangeResult>>> Add([FromBody] AddToCartRequest? request)
    {
        if (request is null || !ModelState.IsValid)
        {
            return InvalidRequest();
        }

        if (User.GetUserId() is not int userId)
        {
            return Unauthorized();
        }

        var result = await cartService.AddItemAsync(userId, request.ProductId, request.Quantity);
        return ToResponse(result, "Đã thêm sản phẩm vào giỏ hàng.");
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<ApiResponse<CartChangeResult>>> UpdateQuantity(int itemId, [FromBody] UpdateCartItemRequest? request)
    {
        if (request is null || !ModelState.IsValid)
        {
            return InvalidRequest();
        }

        if (User.GetUserId() is not int userId)
        {
            return Unauthorized();
        }

        var result = await cartService.UpdateQuantityAsync(userId, itemId, request.Quantity);
        return ToResponse(result, "Đã cập nhật số lượng.");
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<ApiResponse<CartChangeResult>>> Remove(int itemId)
    {
        if (User.GetUserId() is not int userId)
        {
            return Unauthorized();
        }

        var result = await cartService.RemoveItemAsync(userId, itemId);
        return ToResponse(result, "Đã xóa sản phẩm khỏi giỏ hàng.");
    }

    private ActionResult<ApiResponse<CartChangeResult>> ToResponse(ServiceResult<CartChangeResult> result, string successMessage)
    {
        if (!result.Success)
        {
            return BadRequest(ApiResponse<CartChangeResult>.Fail(result.Error ?? "Thao tác không thành công."));
        }

        return Ok(ApiResponse<CartChangeResult>.Ok(result.Data!, successMessage));
    }

    private ActionResult<ApiResponse<CartChangeResult>> InvalidRequest()
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Dữ liệu không hợp lệ." : e.ErrorMessage)
            .ToArray();

        return BadRequest(ApiResponse<CartChangeResult>.Fail("Dữ liệu không hợp lệ.", errors));
    }
}
