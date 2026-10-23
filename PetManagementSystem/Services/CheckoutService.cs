using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Shop;

namespace PetManagementSystem.Services;

public class CheckoutService(
    IClientCartService cartService,
    IClientCartRepository cartRepository,
    IClientOrderRepository orderRepository,
    IUserRepository userRepository,
    ILogger<CheckoutService> logger) : ICheckoutService
{
    private static readonly Regex PhoneRegex = new(CheckoutInputModel.PhonePattern, RegexOptions.Compiled);
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<CheckoutViewModel> BuildAsync(int userId, CheckoutInputModel? input = null)
    {
        var user = await userRepository.GetByIdAsync(userId);

        // Lần đầu vào trang: tự điền sẵn họ tên, email, số điện thoại, địa chỉ từ hồ sơ cá nhân để khách không phải gõ lại.
        if (input is null)
        {
            var user = await userRepository.GetByIdAsync(userId);
            input = new CheckoutInputModel
            {
                FullName = user?.FullName?.Trim() ?? string.Empty,
                Email = user?.Email?.Trim() ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                ShippingAddress = user?.Address?.Trim() ?? string.Empty
            };
        }

        if (string.IsNullOrEmpty(input.PaymentMethod) || !ClientPaymentMethods.Supported.Contains(input.PaymentMethod))
        {
            input.PaymentMethod = ClientPaymentMethods.Supported[0];
        }

        return new CheckoutViewModel
        {
            Cart = await cartService.GetCartAsync(userId),
            Input = input,
            SavedAddresses = await BuildSavedAddressesAsync(userId, user?.Address),
            PaymentOptions = ClientPaymentMethods.Supported.Select(m => new PaymentOptionViewModel
            {
                Value = m,
                Label = PaymentMethods.Label(m),
                Description = ClientPaymentMethods.Description(m)
            }).ToList()
        };
    }

    /// <summary>Địa chỉ trong hồ sơ (nếu có) + tối đa 4 địa chỉ đã giao gần đây, không trùng nhau.</summary>
    private async Task<List<SavedAddressViewModel>> BuildSavedAddressesAsync(int userId, string? profileAddress)
    {
        var result = new List<SavedAddressViewModel>();

        var profile = profileAddress.TrimToNull();
        if (profile is not null)
        {
            result.Add(new SavedAddressViewModel { Address = profile, Source = "Hồ sơ" });
        }

        foreach (var address in await orderRepository.GetRecentShippingAddressesAsync(userId, 5))
        {
            if (result.Count >= 5)
            {
                break;
            }

            if (!result.Any(x => string.Equals(x.Address, address, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new SavedAddressViewModel { Address = address, Source = "Đã dùng" });
            }
        }

        return result;
    }

    public async Task<ServiceResult<int>> PlaceOrderAsync(int userId, CheckoutInputModel input)
    {
        if (!ClientPaymentMethods.Supported.Contains(input.PaymentMethod))
        {
            return ServiceResult<int>.Fail("Phương thức thanh toán không được hỗ trợ", nameof(input.PaymentMethod));
        }

        var fullName = input.FullName.TrimToNull();
        if (fullName is null || fullName.Length > 150)
        {
            return ServiceResult<int>.Fail("Họ tên người nhận là bắt buộc (tối đa 150 ký tự)", nameof(input.FullName));
        }

        var email = input.Email.TrimToNull();
        if (email is null || email.Length > 150 || !EmailValidator.IsValid(email))
        {
            return ServiceResult<int>.Fail("Email không hợp lệ", nameof(input.Email));
        }

        var phone = input.Phone.NormalizePhone();
        if (!PhoneRegex.IsMatch(phone))
        {
            return ServiceResult<int>.Fail(CheckoutInputModel.PhoneError, nameof(input.Phone));
        }

        var address = input.ShippingAddress.TrimToNull();
        if (address is null || address.Length > 255)
        {
            return ServiceResult<int>.Fail("Địa chỉ nhận hàng là bắt buộc (tối đa 255 ký tự)", nameof(input.ShippingAddress));
        }

        var note = input.Note.TrimToNull();
        if (note is not null && note.Length > 255)
        {
            return ServiceResult<int>.Fail("Ghi chú tối đa 255 ký tự", nameof(input.Note));
        }

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null || !string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<int>.Fail("Tài khoản không tồn tại hoặc đã bị khóa");
        }

        try
        {
            await using var transaction = await orderRepository.BeginTransactionAsync();

            var cart = await cartRepository.GetActiveCartAsync(userId, includeItems: true);
            if (cart is null || cart.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Giỏ hàng của bạn đang trống");
            }

            // Kiểm tra lại phía server bằng dữ liệu hiện hành trong CSDL, không dùng giá/số lượng từ client.
            var lines = cart.Items.OrderBy(i => i.ProductId).ToList();
            foreach (var item in lines)
            {
                var product = item.Product;
                if (product is null || product.Status != ProductStatuses.Active)
                {
                    return ServiceResult<int>.Fail($"Sản phẩm \"{product?.ProductName ?? "#" + item.ProductId}\" đã ngừng bán, vui lòng xóa khỏi giỏ hàng");
                }

                if (item.Quantity < 1)
                {
                    return ServiceResult<int>.Fail("Giỏ hàng có số lượng không hợp lệ");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return ServiceResult<int>.Fail($"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho");
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ReceiverName = fullName,
                ReceiverEmail = email,
                ShippingAddress = address,
                Phone = phone,
                Status = OrderStatuses.Pending,
                Note = note
            };

            foreach (var item in lines)
            {
                var price = item.Product!.SellPrice;
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = price,
                    Subtotal = price * item.Quantity
                });
            }

            order.TotalAmount = order.Items.Sum(x => x.Subtotal);
            if (order.TotalAmount > CommerceMapping.MaxMoney)
            {
                return ServiceResult<int>.Fail("Tổng giá trị đơn hàng vượt quá giới hạn cho phép");
            }

            // Chốt giỏ nguyên tử: nếu bấm đặt hàng nhiều lần, chỉ yêu cầu đầu tiên giành được giỏ.
            if (!await cartRepository.TryMarkCheckedOutAsync(cart.Id, userId))
            {
                return ServiceResult<int>.Fail("Giỏ hàng đã được đặt hoặc đã thay đổi. Vui lòng kiểm tra lại đơn hàng của bạn");
            }

            // Trừ kho nguyên tử theo từng sản phẩm; thiếu hàng ở bất kỳ dòng nào thì rollback toàn bộ.
            foreach (var item in lines)
            {
                if (!await orderRepository.TryReserveStockAsync(item.ProductId, item.Quantity))
                {
                    return ServiceResult<int>.Fail($"Sản phẩm \"{item.Product!.ProductName}\" không còn đủ hàng để đặt");
                }
            }

            await orderRepository.AddOrderAsync(order);

            await orderRepository.AddPaymentAsync(new Payment
            {
                OrderId = order.Id,
                PaymentMethod = input.PaymentMethod,
                Amount = order.TotalAmount,
                PaymentDate = order.OrderDate,
                Status = PaymentStatuses.Pending,
                TransactionCode = $"PAY-{order.Id}-{DateTime.Now:yyyyMMddHHmmssfff}"
            });

            // Ghi lại đơn giá đã dùng để đặt hàng vào cart_items.price của giỏ vừa chốt.
            foreach (var item in lines)
            {
                item.Price = item.Product!.SellPrice;
            }

            await cartRepository.TrySaveAsync();

            await transaction.CommitAsync();
            return ServiceResult<int>.Ok(order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Đặt hàng thất bại cho user {UserId}", userId);
            return ServiceResult<int>.Fail("Không thể đặt hàng lúc này, vui lòng thử lại sau");
        }
    }
}
