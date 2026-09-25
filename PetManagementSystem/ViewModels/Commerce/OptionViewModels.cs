namespace PetManagementSystem.ViewModels.Commerce;

/// <summary>Sản phẩm dùng cho ô chọn trong form phiếu nhập / đơn hàng (kèm giá, tồn kho để JS tự điền).</summary>
public class ProductOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
}

public class UserOptionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class OrderOptionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }

    /// <summary>Tổng tiền đã thanh toán thành công của đơn.</summary>
    public decimal PaidAmount { get; set; }
}
