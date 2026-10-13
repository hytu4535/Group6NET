using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<PagedResult<Notification>> GetByUserIdPagedAsync(int userId, int page, int pageSize);
    Task<List<Notification>> GetCustomerNotificationsAsync();
    Task<PagedResult<Notification>> GetCustomerNotificationsPagedAsync(int page, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<int> GetCustomerUnreadCountAsync();
    Task AddAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int userId);
    Task MarkAllCustomerNotificationsAsReadAsync();
}
