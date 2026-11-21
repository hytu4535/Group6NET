using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class OrderBillingService(
    IOrderRepository orderRepository,
    IInvoiceRepository invoiceRepository,
    ILogger<OrderBillingService> logger) : IOrderBillingService
{
    public async Task<bool> EnsureInvoiceAsync(int orderId)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null || order.Status == OrderStatuses.Cancelled || order.Invoice is not null || order.TotalAmount <= 0)
        {
            return false;
        }

        var paid = order.Payments
            .Where(p => p.Status == PaymentStatuses.Completed)
            .Sum(p => p.Amount);

        if (paid < order.TotalAmount)
        {
            return false;
        }

        var created = await invoiceRepository.TryAddAsync(new Invoice
        {
            OrderId = order.Id,
            InvoiceDate = DateTime.Now,
            TotalAmount = order.TotalAmount,
            TaxAmount = Math.Round(order.TotalAmount * InvoiceDefaults.TaxRate, 2),
            IssuedBy = null // null = hệ thống tự lập
        });

        if (created)
        {
            logger.LogInformation("Đã tự lập hóa đơn cho đơn hàng {OrderId}", order.Id);
        }

        return created;
    }
}
