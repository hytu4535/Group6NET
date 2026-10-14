using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Shop;

public class CartLineViewModel
{
    public int ItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Unit { get; set; }

    /// <summary>Đơn giá hiện hành của sản phẩm (lấy từ bảng products).</summary>
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public int StockQuantity { get; set; }

    /// <summary>Lý do dòng này không thể đặt hàng (ngừng bán, hết hàng, vượt tồn kho); null nếu hợp lệ.</summary>
    public string? Issue { get; set; }
    public bool PriceChanged { get; set; }
}

public class CartViewModel
{
    public List<CartLineViewModel> Items { get; set; } = [];
    public decimal TotalAmount => Items.Sum(x => x.LineTotal);
    public int TotalQuantity => Items.Sum(x => x.Quantity);
    public bool HasIssues => Items.Any(x => x.Issue is not null);
}

/// <summary>Kết quả sau mỗi thay đổi giỏ hàng, dùng để cập nhật giao diện.</summary>
public class CartChangeResult
{
    public int? ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal CartTotal { get; set; }
    public int CartQuantity { get; set; }
    public int LineCount { get; set; }
}

public class AddToCartRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm không hợp lệ")]
    public int ProductId { get; set; }

    [Range(1, 9999, ErrorMessage = "Số lượng phải từ 1 đến 9.999")]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(1, 9999, ErrorMessage = "Số lượng phải từ 1 đến 9.999")]
    public int Quantity { get; set; }
}
