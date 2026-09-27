using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository) : IPaymentService
{
    public async Task<PaymentListViewModel> GetListAsync(string? search, string? status, string? method, int page)
    {
        var normalizedStatus = CommerceMapping.NormalizeOption(status, PaymentStatuses.All);
        var normalizedMethod = CommerceMapping.NormalizeOption(method, PaymentMethods.All);

        var result = await paymentRepository.SearchAsync(
            search,
            string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            string.IsNullOrEmpty(normalizedMethod) ? null : normalizedMethod,
            page,
            Paging.DefaultPageSize);

        return new PaymentListViewModel
        {
            Search = search?.Trim(),
            Status = string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            Method = string.IsNullOrEmpty(normalizedMethod) ? null : normalizedMethod,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => x.ToRow()).ToList()
        };
    }

    public async Task<PaymentUpsertViewModel> BuildCreateModelAsync(int? orderId = null, PaymentUpsertViewModel? model = null)
    {
        var isNew = model is null;
        model ??= new PaymentUpsertViewModel();

        var orders = await orderRepository.GetOrdersForPaymentAsync();
        model.OrderOptions = orders.Select(x => x.ToOption()).ToList();

        if (isNew && orderId.HasValue)
        {
            var selected = model.OrderOptions.FirstOrDefault(x => x.Id == orderId.Value);
            if (selected is not null)
            {
                model.OrderId = selected.Id;
                model.Amount = Math.Max(0, selected.TotalAmount - selected.PaidAmount);
            }
        }

        return model;
    }

    public async Task<PaymentUpsertViewModel?> GetForEditAsync(int id)
    {
        var payment = await paymentRepository.GetByIdAsync(id);
        if (payment is null)
        {
            return null;
        }

        return new PaymentUpsertViewModel
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentMethod = payment.PaymentMethod,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status,
            TransactionCode = payment.TransactionCode,
            OrderText = $"#{payment.OrderId} - {CommerceFormat.DisplayName(payment.Order?.User)}"
        };
    }

    public async Task<ServiceResult> CreateAsync(PaymentUpsertViewModel model)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(model.OrderId);
        if (order is null)
        {
            return ServiceResult.Fail("Đơn hàng không tồn tại", nameof(model.OrderId));
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return ServiceResult.Fail("Không thể tạo thanh toán cho đơn hàng đã hủy", nameof(model.OrderId));
        }

        var amount = Math.Round(model.Amount, 2);
        var check = await ValidateAmountAsync(order, amount, model.Status, excludePaymentId: null);
        if (!check.Success)
        {
            return check;
        }

        var code = model.TransactionCode.TrimToNull();
        if (code is null)
        {
            // Cột transaction_code là UNIQUE và SQL Server chỉ cho phép một dòng NULL,
            // nên tự sinh mã khi người dùng không nhập.
            code = GenerateTransactionCode(order.Id);
        }
        else if (await paymentRepository.ExistsTransactionCodeAsync(code))
        {
            return ServiceResult.Fail("Mã giao dịch đã tồn tại", nameof(model.TransactionCode));
        }

        await paymentRepository.AddAsync(new Payment
        {
            OrderId = order.Id,
            PaymentMethod = model.PaymentMethod,
            Amount = amount,
            PaymentDate = model.PaymentDate,
            Status = model.Status,
            TransactionCode = code
        });

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(PaymentUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được giao dịch cần cập nhật");
        }

        var payment = await paymentRepository.GetByIdAsync(model.Id.Value);
        if (payment is null)
        {
            return ServiceResult.Fail("Không tìm thấy giao dịch");
        }

        var order = await orderRepository.GetByIdWithDetailsAsync(payment.OrderId);
        if (order is null)
        {
            return ServiceResult.Fail("Đơn hàng của giao dịch không còn tồn tại");
        }

        var amount = Math.Round(model.Amount, 2);
        var check = await ValidateAmountAsync(order, amount, model.Status, payment.Id);
        if (!check.Success)
        {
            return check;
        }

        var code = model.TransactionCode.TrimToNull() ?? payment.TransactionCode ?? GenerateTransactionCode(order.Id);
        if (await paymentRepository.ExistsTransactionCodeAsync(code, payment.Id))
        {
            return ServiceResult.Fail("Mã giao dịch đã tồn tại", nameof(model.TransactionCode));
        }

        // Không đổi đơn hàng của một giao dịch.
        payment.PaymentMethod = model.PaymentMethod;
        payment.Amount = amount;
        payment.PaymentDate = model.PaymentDate;
        payment.Status = model.Status;
        payment.TransactionCode = code;

        await paymentRepository.UpdateAsync(payment);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var payment = await paymentRepository.GetByIdAsync(id);
        if (payment is null)
        {
            return ServiceResult.Fail("Không tìm thấy giao dịch");
        }

        if (payment.Status is not (PaymentStatuses.Pending or PaymentStatuses.Failed))
        {
            return ServiceResult.Fail("Chỉ được xóa giao dịch đang chờ hoặc thất bại. Giao dịch đã thanh toán/hoàn tiền cần giữ lại để đối soát.");
        }

        await paymentRepository.DeleteAsync(payment);
        return ServiceResult.Ok();
    }

    /// <summary>Tổng các giao dịch đã thanh toán thành công không được vượt giá trị đơn hàng.</summary>
    private async Task<ServiceResult> ValidateAmountAsync(Order order, decimal amount, string status, int? excludePaymentId)
    {
        if (status != PaymentStatuses.Completed)
        {
            return ServiceResult.Ok();
        }

        var alreadyPaid = await paymentRepository.GetCompletedTotalAsync(order.Id, excludePaymentId);
        if (alreadyPaid + amount > order.TotalAmount)
        {
            var remaining = Math.Max(0, order.TotalAmount - alreadyPaid);
            return ServiceResult.Fail(
                $"Số tiền vượt quá giá trị đơn hàng. Số tiền còn phải thu: {CommerceFormat.Money(remaining)}",
                nameof(PaymentUpsertViewModel.Amount));
        }

        return ServiceResult.Ok();
    }

    private static string GenerateTransactionCode(int orderId) =>
        $"PAY-{orderId}-{DateTime.Now:yyyyMMddHHmmssfff}";
}
