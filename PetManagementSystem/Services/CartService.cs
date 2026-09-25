using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class CartService(ICartRepository cartRepository) : ICartService
{
    public async Task<CartListViewModel> GetListAsync(string? search, string? status, int page)
    {
        var normalizedStatus = CommerceMapping.NormalizeOption(status, CartStatuses.All);
        var result = await cartRepository.SearchAsync(
            search,
            string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            page,
            Paging.DefaultPageSize);

        return new CartListViewModel
        {
            Search = search?.Trim(),
            Status = string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new CartRowViewModel
            {
                Id = x.Id,
                CustomerName = CommerceFormat.DisplayName(x.User),
                Username = x.User?.Username,
                Status = x.Status,
                ItemCount = x.Items.Count,
                TotalAmount = x.Items.Sum(i => i.Price * i.Quantity),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList()
        };
    }

    public async Task<CartItemListViewModel> GetItemsAsync(int? cartId, string? search, int page)
    {
        var result = await cartRepository.SearchItemsAsync(cartId, search, page, Paging.DefaultPageSize);

        return new CartItemListViewModel
        {
            CartId = cartId,
            Search = search?.Trim(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(ToRow).ToList()
        };
    }

    public async Task<CartDetailsViewModel?> GetDetailsAsync(int id)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id);
        if (cart is null)
        {
            return null;
        }

        return new CartDetailsViewModel
        {
            Id = cart.Id,
            CustomerName = CommerceFormat.DisplayName(cart.User),
            Username = cart.User?.Username,
            Email = cart.User?.Email,
            Status = cart.Status,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            // Truy vấn có theo dõi thay đổi nên item.Cart / item.Cart.User đã được EF tự gắn.
            Items = cart.Items.OrderBy(x => x.Id).Select(ToRow).ToList()
        };
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var cart = await cartRepository.GetByIdWithItemsAsync(id);
        if (cart is null)
        {
            return ServiceResult.Fail("Không tìm thấy giỏ hàng");
        }

        await cartRepository.DeleteAsync(cart);
        return ServiceResult.Ok();
    }

    private static CartItemRowViewModel ToRow(CartItem item) => new()
    {
        Id = item.Id,
        CartId = item.CartId,
        CustomerName = CommerceFormat.DisplayName(item.Cart?.User),
        CartStatus = item.Cart?.Status ?? string.Empty,
        ProductId = item.ProductId,
        ProductName = item.Product?.ProductName ?? $"#{item.ProductId}",
        Unit = item.Product?.Unit,
        Quantity = item.Quantity,
        Price = item.Price
    };
}
