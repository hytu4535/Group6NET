using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public class ClientOrderService(
    IClientOrderRepository orderRepository,
    ILogger<ClientOrderService> logger) : IClientOrderService
{
    public async Task<MyOrderListViewModel> GetOrdersAsync(int userId, MyOrdersQuery query)
    {
        var status = CommerceMapping.NormalizeOption(query.Status, OrderStatuses.All);
        var result = await orderRepository.SearchOwnedAsync(
            userId,
            status.Length == 0 ? null : status,
            query.From,
            query.To,
            query.Page,
            Paging.DefaultPageSize);

        return new MyOrderListViewModel
        {
            Status = status.Length == 0 ? null : status,
            From = query.From,
            To = query.To,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(o => new MyOrderRowViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.Items.Count,
                PaymentStatus = SummarizePayment(o)
            }).ToList()
        };
    }

    public async Task<MyOrderDetailsViewModel?> GetOrderAsync(int userId, int orderId)
    {
        var order = await orderRepository.GetOwnedDetailsAsync(userId, orderId);
        if (order is null)
        {
            return null;
        }

        var paid = order.Payments.Where(p => p.Status == PaymentStatuses.Completed).Sum(p => p.Amount);

        return new MyOrderDetailsViewModel
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            ReceiverName = order.ReceiverName,
            ReceiverEmail = order.ReceiverEmail,
            ShippingAddress = order.ShippingAddress,
            Phone = order.Phone,
            Note = order.Note,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaidAmount = paid,
            CanCancel = order.Status == OrderStatuses.Pending && paid == 0,
            HasInvoice = order.Invoice is not null,
            Items = order.Items.OrderBy(i => i.Id).Select(ToItem).ToList(),
            Payments = order.Payments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id).Select(p => new MyPaymentViewModel
            {
                Id = p.Id,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                TransactionCode = p.TransactionCode
            }).ToList()
        };
    }

    public async Task<ServiceResult> CancelAsync(int userId, int orderId)
    {
        var order = await orderRepository.GetOwnedDetailsAsync(userId, orderId);
        if (order is null)
        {
            return ServiceResult.Fail("Không tìm thấy đơn hàng");
        }

        if (order.Status != OrderStatuses.Pending)
        {
            return ServiceResult.Fail("Chỉ có thể hủy đơn khi đơn đang chờ xác nhận. Vui lòng liên hệ cửa hàng để được hỗ trợ.");
        }

        try
        {
            await using var transaction = await orderRepository.BeginTransactionAsync();

            // Chuyển trạng thái có điều kiện (đang pending) để tránh xung đột với nhân viên vừa xác nhận đơn.
            if (!await orderRepository.TryCancelPendingAsync(userId, orderId))
            {
                return ServiceResult.Fail("Đơn hàng vừa được cập nhật trạng thái nên không thể hủy. Vui lòng tải lại trang.");
            }

            if (await orderRepository.HasCompletedPaymentAsync(orderId))
            {
                return ServiceResult.Fail("Đơn hàng đã có thanh toán thành công. Vui lòng liên hệ cửa hàng để được hoàn tiền và hủy đơn.");
            }

            foreach (var item in order.Items.OrderBy(i => i.ProductId))
            {
                await orderRepository.ReleaseStockAsync(item.ProductId, item.Quantity);
            }

            await orderRepository.FailPendingPaymentsAsync(orderId);

            await transaction.CommitAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Hủy đơn {OrderId} thất bại cho user {UserId}", orderId, userId);
            return ServiceResult.Fail("Không thể hủy đơn lúc này, vui lòng thử lại sau");
        }
    }

    public async Task<MyInvoiceViewModel?> GetInvoiceAsync(int userId, int orderId)
    {
        var invoice = await orderRepository.GetOwnedInvoiceAsync(userId, orderId);
        if (invoice?.Order is null)
        {
            return null;
        }

        return new MyInvoiceViewModel
        {
            InvoiceId = invoice.Id,
            OrderId = invoice.OrderId,
            InvoiceDate = invoice.InvoiceDate,
            CustomerName = CommerceFormat.DisplayName(invoice.Order.User),
            Email = invoice.Order.User?.Email,
            Phone = invoice.Order.Phone,
            ShippingAddress = invoice.Order.ShippingAddress,
            TotalAmount = invoice.TotalAmount,
            TaxAmount = invoice.TaxAmount,
            Items = invoice.Order.Items.OrderBy(i => i.Id).Select(ToItem).ToList()
        };
    }

    private static MyOrderItemViewModel ToItem(OrderItem item) => new()
    {
        ProductId = item.ProductId,
        ProductName = item.Product?.ProductName ?? $"#{item.ProductId}",
        ImageUrl = item.Product?.ImageUrl,
        Unit = item.Product?.Unit,
        Quantity = item.Quantity,
        Price = item.Price,
        Subtotal = item.Subtotal
    };

    /// <summary>Trạng thái thanh toán tổng hợp của đơn: đã đủ tiền -> completed, ngược lại lấy giao dịch gần nhất.</summary>
    private static string? SummarizePayment(Order order)
    {
        if (order.Payments.Count == 0)
        {
            return null;
        }

        var paid = order.Payments.Where(p => p.Status == PaymentStatuses.Completed).Sum(p => p.Amount);
        if (order.TotalAmount > 0 && paid >= order.TotalAmount)
        {
            return PaymentStatuses.Completed;
        }

        return order.Payments.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id).First().Status;
    }
}
