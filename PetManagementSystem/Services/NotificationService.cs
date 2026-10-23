using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUserRepository userRepository) : INotificationService
{
    public Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return notificationRepository.GetByUserIdAsync(userId);
    }

    public Task<PagedResult<Notification>> GetByUserIdPagedAsync(int userId, int page, int pageSize)
    {
        return notificationRepository.GetByUserIdPagedAsync(userId, page, pageSize);
    }

    public Task<List<Notification>> GetCustomerNotificationsAsync()
    {
        return notificationRepository.GetCustomerNotificationsAsync();
    }

    public Task<PagedResult<Notification>> GetCustomerNotificationsPagedAsync(int page, int pageSize)
    {
        return notificationRepository.GetCustomerNotificationsPagedAsync(page, pageSize);
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return notificationRepository.GetUnreadCountAsync(userId);
    }

    public Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        return notificationRepository.MarkAsReadAsync(userId, notificationId);
    }

    public Task<int> GetCustomerUnreadCountAsync()
    {
        return notificationRepository.GetCustomerUnreadCountAsync();
    }

    public async Task SendNotificationAsync(int userId, string message, string type = "system")
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Nội dung thông báo không được để trống.", nameof(message));
        }

        await notificationRepository.AddAsync(new Notification
        {
            UserId = userId,
            AppointmentId = null,
            Message = message.Trim(),
            Type = NormalizeType(type),
            IsRead = false,
            CreatedAt = DateTime.Now
        });
    }

    public async Task SendNotificationToAllCustomersAsync(string message, string type = "system")
    {
        var users = await userRepository.GetAllWithRoleAsync();
        var recipients = users
            .Where(user => string.Equals(user.Role?.Name, "member", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var recipient in recipients)
        {
            await SendNotificationAsync(recipient.Id, message, type);
        }
    }

    public Task MarkAsReadAsync(int notificationId)
    {
        return notificationRepository.MarkAsReadAsync(notificationId);
    }

    public Task MarkAllAsReadAsync(int userId)
    {
        return notificationRepository.MarkAllAsReadAsync(userId);
    }

    public Task MarkAllCustomerNotificationsAsReadAsync()
    {
        return notificationRepository.MarkAllCustomerNotificationsAsReadAsync();
    }

    private static string NormalizeType(string type)
    {
        return type.Trim().ToLowerInvariant() == "promotion"
            ? "promotion"
            : "system";
    }

}
