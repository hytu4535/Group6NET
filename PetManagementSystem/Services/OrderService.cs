using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Commerce;

namespace PetManagementSystem.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUserRepository userRepository) : IOrderService
{
    public async Task<OrderListViewModel> GetListAsync(string? search, string? status, DateTime? from, DateTime? to, int page)
    {
        var normalizedStatus = CommerceMapping.NormalizeOption(status, OrderStatuses.All);
        var result = await orderRepository.SearchAsync(
            search,
            string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            from,
            to,
            page,
            Paging.DefaultPageSize);

        return new OrderListViewModel
        {
            Search = search?.Trim(),
            Status = string.IsNullOrEmpty(normalizedStatus) ? null : normalizedStatus,
            From = from,
            To = to,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new OrderRowViewModel
            {
                Id = x.Id,
                CustomerName = CommerceFormat.DisplayName(x.User),
                Phone = x.Phone,
                OrderDate = x.OrderDate,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                ItemCount = x.Items.Count
            }).ToList()
        };
    }

    public async Task<OrderItemListViewModel> GetItemsAsync(int? orderId, string? search, int page)
    {
        var result = await orderRepository.SearchItemsAsync(orderId, search, page, Paging.DefaultPageSize);

        return new OrderItemListViewModel
        {
            OrderId = orderId,
            Search = search?.Trim(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => x.ToRow()).ToList()
        };
    }

    public async Task<OrderDetailsViewModel?> GetDetailsAsync(int id)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null)
        {
            return null;
        }

        // Truy vấn có theo dõi thay đổi nên item.Order / payment.Order đã được EF tự gắn.
        return new OrderDetailsViewModel
        {
            Id = order.Id,
            CustomerName = CommerceFormat.DisplayName(order.User),
            Username = order.User?.Username,
            Email = order.User?.Email,
            Phone = order.Phone,
            ShippingAddress = order.ShippingAddress,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Note = order.Note,
            PaidAmount = order.Payments.Where(p => p.Status == PaymentStatuses.Completed).Sum(p => p.Amount),
            CanEdit = OrderStatuses.CanEdit(order.Status),
            NextStatuses = OrderStatuses.NextStatuses(order.Status).ToList(),
            InvoiceId = order.Invoice?.Id,
            Items = order.Items.OrderBy(x => x.Id).Select(x => x.ToRow()).ToList(),
            Payments = order.Payments.OrderByDescending(x => x.PaymentDate).Select(x => x.ToRow()).ToList()
        };
    }

    public async Task<OrderCreateViewModel> BuildCreateModelAsync(OrderCreateViewModel? model = null)
    {
        model ??= new OrderCreateViewModel();

        var users = await userRepository.GetAllWithRoleAsync();
        model.UserOptions = users
            .OrderBy(x => CommerceFormat.DisplayName(x))
            .Select(x => new UserOptionViewModel
            {
                Id = x.Id,
                Text = string.IsNullOrWhiteSpace(x.FullName) ? x.Username : $"{x.FullName} ({x.Username})",
                Phone = x.PhoneNumber,
                Address = x.Address
            }).ToList();

        var products = await productRepository.GetOptionsAsync(onlyActive: true);
        model.ProductOptions = products
            .Where(x => x.StockQuantity > 0)
            .Select(x => x.ToOption())
            .ToList();

        return model;
    }

    public async Task<ServiceResult<int>> CreateAsync(OrderCreateViewModel model)
    {
        if (model.Lines.Count == 0)
        {
            return ServiceResult<int>.Fail("Đơn hàng cần có ít nhất một sản phẩm");
        }

        var user = await userRepository.GetByIdAsync(model.UserId);
        if (user is null || !string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<int>.Fail("Khách hàng không tồn tại hoặc đã bị khóa", nameof(model.UserId));
        }

        if (model.Lines.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
        {
            return ServiceResult<int>.Fail("Mỗi sản phẩm chỉ được xuất hiện một lần trong đơn hàng. Hãy gộp số lượng vào cùng một dòng.");
        }

        var products = (await productRepository.GetByIdsAsync(model.Lines.Select(x => x.ProductId)))
            .ToDictionary(x => x.Id);

        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.Now,
            ShippingAddress = model.ShippingAddress.Trim(),
            Phone = model.Phone.Trim(),
            Status = OrderStatuses.Pending,
            Note = model.Note.TrimToNull()
        };

        foreach (var line in model.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return ServiceResult<int>.Fail("Có sản phẩm trong đơn không còn tồn tại");
            }

            if (product.Status != ProductStatuses.Active)
            {
                return ServiceResult<int>.Fail($"Sản phẩm \"{product.ProductName}\" đã ngừng bán");
            }

            if (product.StockQuantity < line.Quantity)
            {
                return ServiceResult<int>.Fail($"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho");
            }

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                Price = product.SellPrice,
                Subtotal = product.SellPrice * line.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.Subtotal);
        if (order.TotalAmount > CommerceMapping.MaxMoney)
        {
            return ServiceResult<int>.Fail("Tổng giá trị đơn hàng vượt quá giới hạn cho phép");
        }

        // Trừ tồn kho; các thay đổi này được lưu cùng một lần SaveChanges với đơn hàng.
        foreach (var line in model.Lines)
        {
            var product = products[line.ProductId];
            product.StockQuantity -= line.Quantity;
            product.UpdatedAt = DateTime.Now;
        }

        await orderRepository.AddAsync(order);
        return ServiceResult<int>.Ok(order.Id);
    }

    public async Task<OrderEditViewModel?> GetForEditAsync(int id)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null)
        {
            return null;
        }

        return new OrderEditViewModel
        {
            Id = order.Id,
            CustomerName = CommerceFormat.DisplayName(order.User),
            ShippingAddress = order.ShippingAddress,
            Phone = order.Phone,
            Note = order.Note
        };
    }

    public async Task<ServiceResult> UpdateAsync(OrderEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return ServiceResult.Fail("Không xác định được đơn hàng cần cập nhật");
        }

        var order = await orderRepository.GetByIdWithDetailsAsync(model.Id.Value);
        if (order is null)
        {
            return ServiceResult.Fail("Không tìm thấy đơn hàng");
        }

        if (!OrderStatuses.CanEdit(order.Status))
        {
            return ServiceResult.Fail("Chỉ được sửa thông tin giao hàng khi đơn đang chờ xác nhận hoặc đã xác nhận");
        }

        order.ShippingAddress = model.ShippingAddress.Trim();
        order.Phone = model.Phone.Trim();
        order.Note = model.Note.TrimToNull();

        await orderRepository.UpdateAsync(order);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangeStatusAsync(int id, string newStatus)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null)
        {
            return ServiceResult.Fail("Không tìm thấy đơn hàng");
        }

        if (!OrderStatuses.CanTransition(order.Status, newStatus))
        {
            return ServiceResult.Fail(
                $"Không thể chuyển đơn từ \"{OrderStatuses.Label(order.Status)}\" sang \"{OrderStatuses.Label(newStatus)}\"");
        }

        if (newStatus == OrderStatuses.Cancelled)
        {
            if (order.Payments.Any(p => p.Status == PaymentStatuses.Completed))
            {
                return ServiceResult.Fail("Đơn hàng đã có thanh toán thành công. Hãy chuyển giao dịch sang \"Đã hoàn tiền\" trước khi hủy đơn.");
            }

            // Hoàn lại tồn kho cho các sản phẩm trong đơn.
            var products = (await productRepository.GetByIdsAsync(order.Items.Select(x => x.ProductId)))
                .ToDictionary(x => x.Id);

            foreach (var item in order.Items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }
            }
        }

        order.Status = newStatus;
        await orderRepository.UpdateAsync(order);
        return ServiceResult.Ok();
    }
}
