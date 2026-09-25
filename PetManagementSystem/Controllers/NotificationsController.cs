using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Repositories;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

[Authorize]
public class NotificationsController(
    IUserRepository userRepository,
    INotificationService notificationService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var customers = (await userRepository.GetAllWithRoleAsync())
            .Where(user => string.Equals(user.Role?.Name, "member", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var pagedNotifications = await notificationService.GetCustomerNotificationsPagedAsync(page, pageSize);

        ViewData["Title"] = "Thông báo";
        ViewData["Active"] = "Notifications";
        ViewData["ActiveParent"] = "CustomerCare";
        ViewData["Customers"] = customers;
        ViewData["UnreadCount"] = await notificationService.GetCustomerUnreadCountAsync();
        ViewData["Page"] = pagedNotifications.Page;
        ViewData["PageSize"] = pagedNotifications.PageSize;
        ViewData["TotalItems"] = pagedNotifications.TotalItems;
        return View("~/Views/Admin/Notifications.cshtml", pagedNotifications.Items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNotification(
        int? userId, string message, string type, bool sendToAll, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập nội dung thông báo.";
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }

        if (sendToAll)
        {
            await notificationService.SendNotificationToAllCustomersAsync(message, type);
            TempData["SuccessMessage"] = "Đã gửi thông báo cho tất cả khách hàng.";
        }
        else if (userId.HasValue)
        {
            var isCustomer = (await userRepository.GetAllWithRoleAsync())
                .Any(user => user.Id == userId.Value
                    && string.Equals(user.Role?.Name, "member", StringComparison.OrdinalIgnoreCase));
            if (!isCustomer)
            {
                TempData["ErrorMessage"] = "Người nhận không phải là tài khoản khách hàng hợp lệ.";
                return RedirectToAction(nameof(Index), new { page, pageSize });
            }

            await notificationService.SendNotificationAsync(userId.Value, message, type);
            TempData["SuccessMessage"] = "Đã gửi thông báo cho khách hàng.";
        }
        else
        {
            TempData["ErrorMessage"] = "Vui lòng chọn khách hàng hoặc chọn gửi cho tất cả.";
        }

        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id, int page = 1, int pageSize = 10)
    {
        var notifications = await notificationService.GetCustomerNotificationsAsync();
        if (!notifications.Any(notification => notification.Id == id))
        {
            return NotFound();
        }

        await notificationService.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead(int page = 1, int pageSize = 10)
    {
        await notificationService.MarkAllCustomerNotificationsAsReadAsync();
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }
}