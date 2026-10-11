using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

/// <summary>Các hàm map dùng chung giữa các service Commerce.</summary>
internal static class CommerceMapping
{
    public const decimal MaxMoney = 999_999_999_999m; // giới hạn cột DECIMAL(14,2) dùng an toàn

    public static string NormalizeOption(string? value, IEnumerable<string> allowed) =>
        value is not null && allowed.Contains(value) ? value : string.Empty;

    public static OrderOptionViewModel ToOption(this Order order) => new()
    {
        Id = order.Id,
        Text = $"#{order.Id} - {CommerceFormat.DisplayName(order.User)} - {CommerceFormat.Money(order.TotalAmount)}",
        TotalAmount = order.TotalAmount,
        PaidAmount = order.Payments
            .Where(p => p.Status == PaymentStatuses.Completed)
            .Sum(p => p.Amount)
    };

    public static PaymentRowViewModel ToRow(this Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        CustomerName = CommerceFormat.DisplayName(payment.Order?.User),
        PaymentMethod = payment.PaymentMethod,
        Amount = payment.Amount,
        PaymentDate = payment.PaymentDate,
        Status = payment.Status,
        TransactionCode = payment.TransactionCode
    };

    public static OrderItemRowViewModel ToRow(this OrderItem item) => new()
    {
        Id = item.Id,
        OrderId = item.OrderId,
        CustomerName = CommerceFormat.DisplayName(item.Order?.User),
        OrderStatus = item.Order?.Status ?? string.Empty,
        ProductId = item.ProductId,
        ProductName = item.Product?.ProductName ?? $"#{item.ProductId}",
        Unit = item.Product?.Unit,
        Quantity = item.Quantity,
        Price = item.Price,
        Subtotal = item.Subtotal
    };

    public static ProductOptionViewModel ToOption(this Product product) => new()
    {
        Id = product.Id,
        Name = product.ProductName,
        Unit = product.Unit,
        ImportPrice = product.ImportPrice,
        SellPrice = product.SellPrice,
        StockQuantity = product.StockQuantity
    };
}
