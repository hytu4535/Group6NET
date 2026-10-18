using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public class ClientCartService(IClientCartRepository cartRepository) : IClientCartService
{
    private const string SaveFailedMessage = "Không thể cập nhật giỏ hàng lúc này, vui lòng thử lại.";

    public async Task<CartViewModel> GetCartAsync(int userId)
    {
        var cart = await cartRepository.GetActiveCartAsync(userId, includeItems: true);
        return ToViewModel(cart);
    }

    public async Task<ServiceResult<CartChangeResult>> AddItemAsync(int userId, int productId, int quantity)
    {
        if (quantity < 1)
        {
            return ServiceResult<CartChangeResult>.Fail("Số lượng phải lớn hơn 0");
        }

        var product = await cartRepository.GetProductAsync(productId);
        if (product is null || product.Status != ProductStatuses.Active)
        {
            return ServiceResult<CartChangeResult>.Fail("Sản phẩm không tồn tại hoặc đã ngừng bán");
        }

        if (product.StockQuantity <= 0)
        {
            return ServiceResult<CartChangeResult>.Fail($"Sản phẩm \"{product.ProductName}\" đã hết hàng");
        }

        var cart = await cartRepository.GetActiveCartAsync(userId, includeItems: true)
                   ?? await cartRepository.CreateActiveCartAsync(userId);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        var newQuantity = (existing?.Quantity ?? 0) + quantity;
        if (newQuantity > product.StockQuantity)
        {
            var inCart = existing?.Quantity ?? 0;
            var message = inCart > 0
                ? $"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho (bạn đã có {inCart} trong giỏ)"
                : $"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho";
            return ServiceResult<CartChangeResult>.Fail(message, nameof(AddToCartRequest.Quantity));
        }

        if (existing is null)
        {
            existing = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = newQuantity,
                Price = product.SellPrice
            };
            cartRepository.AddItem(existing);
        }
        else
        {
            existing.Quantity = newQuantity;
            existing.Price = product.SellPrice;
        }

        cart.UpdatedAt = DateTime.Now;
        if (!await cartRepository.TrySaveAsync())
        {
            return ServiceResult<CartChangeResult>.Fail(SaveFailedMessage);
        }

        return await SummarizeAsync(userId, existing.Id, product.SellPrice);
    }

    public async Task<ServiceResult<CartChangeResult>> UpdateQuantityAsync(int userId, int itemId, int quantity)
    {
        if (quantity < 1)
        {
            return ServiceResult<CartChangeResult>.Fail("Số lượng phải lớn hơn 0", nameof(UpdateCartItemRequest.Quantity));
        }

        var item = await cartRepository.GetActiveItemAsync(userId, itemId);
        if (item is null)
        {
            return ServiceResult<CartChangeResult>.Fail("Không tìm thấy sản phẩm trong giỏ hàng");
        }

        var product = item.Product;
        if (product is null || product.Status != ProductStatuses.Active)
        {
            return ServiceResult<CartChangeResult>.Fail("Sản phẩm đã ngừng bán, vui lòng xóa khỏi giỏ hàng");
        }

        if (quantity > product.StockQuantity)
        {
            return ServiceResult<CartChangeResult>.Fail(
                $"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho",
                nameof(UpdateCartItemRequest.Quantity));
        }

        item.Quantity = quantity;
        item.Price = product.SellPrice;
        item.Cart!.UpdatedAt = DateTime.Now;

        if (!await cartRepository.TrySaveAsync())
        {
            return ServiceResult<CartChangeResult>.Fail(SaveFailedMessage);
        }

        return await SummarizeAsync(userId, item.Id, product.SellPrice);
    }

    public async Task<ServiceResult<CartChangeResult>> RemoveItemAsync(int userId, int itemId)
    {
        var item = await cartRepository.GetActiveItemAsync(userId, itemId);
        if (item is null)
        {
            return ServiceResult<CartChangeResult>.Fail("Không tìm thấy sản phẩm trong giỏ hàng");
        }

        item.Cart!.UpdatedAt = DateTime.Now;
        cartRepository.RemoveItem(item);

        if (!await cartRepository.TrySaveAsync())
        {
            return ServiceResult<CartChangeResult>.Fail(SaveFailedMessage);
        }

        return await SummarizeAsync(userId, null, 0);
    }

    private async Task<ServiceResult<CartChangeResult>> SummarizeAsync(int userId, int? itemId, decimal unitPrice)
    {
        var cart = await cartRepository.GetActiveCartAsync(userId, includeItems: true);
        var view = ToViewModel(cart);
        var line = itemId.HasValue ? view.Items.FirstOrDefault(x => x.ItemId == itemId.Value) : null;

        return ServiceResult<CartChangeResult>.Ok(new CartChangeResult
        {
            ItemId = itemId,
            Quantity = line?.Quantity ?? 0,
            LineTotal = line?.LineTotal ?? (itemId.HasValue ? unitPrice : 0),
            CartTotal = view.TotalAmount,
            CartQuantity = view.TotalQuantity,
            LineCount = view.Items.Count
        });
    }

    /// <summary>Đơn giá luôn lấy từ products.sell_price hiện hành; cart_items.price chỉ là giá ghi nhận ở lần cập nhật gần nhất.</summary>
    internal static CartViewModel ToViewModel(Cart? cart)
    {
        var model = new CartViewModel();
        if (cart is null)
        {
            return model;
        }

        foreach (var item in cart.Items.OrderBy(i => i.Id))
        {
            var product = item.Product;
            var line = new CartLineViewModel
            {
                ItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = product?.ProductName ?? $"#{item.ProductId}",
                ImageUrl = product?.ImageUrl,
                Unit = product?.Unit,
                UnitPrice = product?.SellPrice ?? item.Price,
                Quantity = item.Quantity,
                StockQuantity = product?.StockQuantity ?? 0,
                PriceChanged = product is not null && product.SellPrice != item.Price
            };

            if (product is null || product.Status != ProductStatuses.Active)
            {
                line.Issue = "Sản phẩm đã ngừng bán";
            }
            else if (product.StockQuantity <= 0)
            {
                line.Issue = "Sản phẩm đã hết hàng";
            }
            else if (item.Quantity > product.StockQuantity)
            {
                line.Issue = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho";
            }

            model.Items.Add(line);
        }

        return model;
    }
}
