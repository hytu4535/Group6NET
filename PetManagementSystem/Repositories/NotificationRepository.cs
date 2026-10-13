using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class NotificationRepository(AppDbContext dbContext) : INotificationRepository
{
    private static IQueryable<Notification> CustomerNotifications(AppDbContext context)
    {
        return context.Notifications
            .Where(notification => notification.User!.Role!.Name == "member"
                && (notification.AppointmentId == null || notification.AppointmentId == 0)
                && (notification.Type == "promotion" || notification.Type == "system"));
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
    {
        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId
                && (notification.AppointmentId == null || notification.AppointmentId == 0)
                && (notification.Type == "promotion" || notification.Type == "system"))
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .ToListAsync();
    }

    public async Task<PagedResult<Notification>> GetByUserIdPagedAsync(int userId, int page, int pageSize)
    {
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId
                && (notification.AppointmentId == null || notification.AppointmentId == 0)
                && (notification.Type == "promotion" || notification.Type == "system"));
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Notification>(items, totalItems, page, pageSize);
    }

    public async Task<List<Notification>> GetCustomerNotificationsAsync()
    {
        return await CustomerNotifications(dbContext)
            .AsNoTracking()
            .Include(notification => notification.User)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .ToListAsync();
    }

    public async Task<PagedResult<Notification>> GetCustomerNotificationsPagedAsync(int page, int pageSize)
    {
        var query = CustomerNotifications(dbContext)
            .AsNoTracking()
            .Include(notification => notification.User);
        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Notification>(items, totalItems, page, pageSize);
    }

    public Task<int> GetUnreadCountAsync(int userId)
    {
        return dbContext.Notifications
            .CountAsync(notification => notification.UserId == userId
                && !notification.IsRead
                && (notification.AppointmentId == null || notification.AppointmentId == 0)
                && (notification.Type == "promotion" || notification.Type == "system"));
    }

    public Task<int> GetCustomerUnreadCountAsync()
    {
        return CustomerNotifications(dbContext)
            .CountAsync(notification => !notification.IsRead);
    }

    public async Task AddAsync(Notification notification)
    {
        await dbContext.Notifications.AddAsync(notification);
        await dbContext.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await dbContext.Notifications.FindAsync(notificationId);
        if (notification is null || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(item =>
            item.Id == notificationId &&
            item.UserId == userId &&
            (item.AppointmentId == null || item.AppointmentId == 0) &&
            (item.Type == "promotion" || item.Type == "system"));
        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await dbContext.SaveChangesAsync();
        }
        return true;
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await dbContext.Notifications
            .Where(notification => notification.UserId == userId
                && !notification.IsRead
                && (notification.AppointmentId == null || notification.AppointmentId == 0)
                && (notification.Type == "promotion" || notification.Type == "system"))
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task MarkAllCustomerNotificationsAsReadAsync()
    {
        var notifications = await CustomerNotifications(dbContext)
            .Where(notification => !notification.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await dbContext.SaveChangesAsync();
    }
}
