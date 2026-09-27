using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IOrderRepository orderRepository) : IInvoiceService
{
    public async Task<InvoiceListViewModel> GetListAsync(string? search, DateTime? from, DateTime? to, int page)
    {
        var result = await invoiceRepository.SearchAsync(search, from, to, page, Paging.DefaultPageSize);

        return new InvoiceListViewModel
        {
            Search = search?.Trim(),
            From = from,
            To = to,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new InvoiceRowViewModel
            {
                Id = x.Id,
                OrderId = x.OrderId,
                CustomerName = CommerceFormat.DisplayName(x.Order?.User),
                InvoiceDate = x.InvoiceDate,
                TotalAmount = x.TotalAmount,
                TaxAmount = x.TaxAmount,
                IssuedByName = x.IssuedByUser is null ? "-" : CommerceFormat.DisplayName(x.IssuedByUser)
            }).ToList()
        };
    }

    public async Task<InvoiceDetailsViewModel?> GetDetailsAsync(int id)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id);
        if (invoice is null)
        {
            return null;
        }

        return new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            CustomerName = CommerceFormat.DisplayName(invoice.Order?.User),
            Email = invoice.Order?.User?.Email,
            Phone = invoice.Order?.Phone ?? string.Empty,
            ShippingAddress = invoice.Order?.ShippingAddress ?? string.Empty,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            TaxAmount = invoice.TaxAmount,
            IssuedByName = invoice.IssuedByUser is null ? "-" : CommerceFormat.DisplayName(invoice.IssuedByUser),
            Items = invoice.Order?.Items.OrderBy(x => x.Id).Select(x => x.ToRow()).ToList() ?? []
        };
    }

    public async Task<InvoiceUpsertViewModel> BuildCreateModelAsync(int? orderId = null, InvoiceUpsertViewModel? model = null)
    {
        var isNew = model is null;
        model ??= new InvoiceUpsertViewModel();

        var orders = await orderRepository.GetOrdersForInvoiceAsync();
        model.OrderOptions = orders.Select(x => x.ToOption()).ToList();

        if (isNew && orderId.HasValue)
        {
            var selected = model.OrderOptions.FirstOrDefault(x => x.Id == orderId.Value);
            if (selected is not null)
            {
                model.OrderId = selected.Id;
                model.TotalAmount = selected.TotalAmount;
            }
        }

        return model;
    }

    public async Task<InvoiceUpsertViewModel?> GetForEditAsync(int id)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id);
        if (invoice is null)
        {
            return null;
        }

        return new InvoiceUpsertViewModel
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            InvoiceDate = invoice.InvoiceDate,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            OrderText = $"#{invoice.OrderId} - {CommerceFormat.DisplayName(invoice.Order?.User)}"
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(InvoiceUpsertViewModel model, int? issuedBy)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(model.OrderId);
        if (order is null)
        {
            return ServiceResult<int>.Fail("Đơn hàng không tồn tại", nameof(model.OrderId));
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return ServiceResult<int>.Fail("Không thể lập hóa đơn cho đơn hàng đã hủy", nameof(model.OrderId));
        }

        if (await invoiceRepository.ExistsForOrderAsync(order.Id))
        {
            return ServiceResult<int>.Fail("Đơn hàng này đã có hóa đơn", nameof(model.OrderId));
        }

        var tax = Math.Round(model.TaxAmount, 2);
        if (tax > order.TotalAmount)
        {
            return ServiceResult<int>.Fail("Tiền thuế không được lớn hơn tổng tiền hóa đơn", nameof(model.TaxAmount));
        }

        var invoice = new Invoice
        {
            OrderId = order.Id,
            InvoiceDate = model.InvoiceDate,
            TotalAmount = order.TotalAmount,
            TaxAmount = tax,
            IssuedBy = issuedBy
        };

        await invoiceRepository.AddAsync(invoice);
        return ServiceResult<int>.Ok(invoice.Id);
    }

    public async Task<ServiceResult> UpdateAsync(InvoiceUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được hóa đơn cần cập nhật");
        }

        var invoice = await invoiceRepository.GetByIdAsync(model.Id.Value);
        if (invoice is null)
        {
            return ServiceResult.Fail("Không tìm thấy hóa đơn");
        }

        var tax = Math.Round(model.TaxAmount, 2);
        if (tax > invoice.TotalAmount)
        {
            return ServiceResult.Fail("Tiền thuế không được lớn hơn tổng tiền hóa đơn", nameof(model.TaxAmount));
        }

        // Tổng tiền là số liệu chốt theo đơn hàng lúc lập hóa đơn nên không cho sửa.
        invoice.InvoiceDate = model.InvoiceDate;
        invoice.TaxAmount = tax;

        await invoiceRepository.UpdateAsync(invoice);
        return ServiceResult.Ok();
    }
}
