using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Helpers;
using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

public class DashboardRepository(AppDbContext dbContext) : IDashboardRepository
{
    private const string CustomerRoleName = "member";

    public Task<int> CountPetsAsync() => dbContext.Pets.AsNoTracking().CountAsync();

    public Task<int> CountActiveCustomersAsync() =>
        dbContext.Users.AsNoTracking()
            .CountAsync(u => u.Status == "active" && u.Role != null && u.Role.Name == CustomerRoleName);

    public Task<int> CountAppointmentsByStatusAsync(byte status) =>
        dbContext.Appointments.AsNoTracking().CountAsync(a => a.Status == status);

    public Task<int> CountOpenAppointmentsOnAsync(DateTime day)
    {
        var start = day.Date;
        var end = start.AddDays(1);
        return dbContext.Appointments.AsNoTracking()
            .CountAsync(a => a.AppointmentDate >= start && a.AppointmentDate < end && (a.Status == 1 || a.Status == 2));
    }

    public Task<int> CountOrdersByStatusAsync(string status) =>
        dbContext.Orders.AsNoTracking().CountAsync(o => o.Status == status);

    public Task<int> CountOrdersSinceAsync(DateTime from) =>
        dbContext.Orders.AsNoTracking().CountAsync(o => o.OrderDate >= from);

    public Task<int> CountFeedbacksByStatusAsync(string status) =>
        dbContext.Feedbacks.AsNoTracking().CountAsync(f => f.Status == status);

    public async Task<decimal[]> GetMonthlyRevenueAsync(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);

        var rows = await dbContext.Payments.AsNoTracking()
            .Where(p => p.Status == PaymentStatuses.Completed && p.PaymentDate >= start && p.PaymentDate < end)
            .GroupBy(p => p.PaymentDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var result = new decimal[12];
        foreach (var row in rows)
        {
            result[row.Month - 1] = row.Total;
        }

        return result;
    }

    public Task<List<Order>> GetRecentOrdersAsync(int take) =>
        dbContext.Orders.AsNoTracking()
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .Take(take)
            .ToListAsync();

    public Task<List<Appointment>> GetOpenAppointmentsOnAsync(DateTime day, int take)
    {
        var start = day.Date;
        var end = start.AddDays(1);
        return dbContext.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.User)
            .Include(a => a.AppointmentServices).ThenInclude(s => s.Service)
            .Where(a => a.AppointmentDate >= start && a.AppointmentDate < end && (a.Status == 1 || a.Status == 2))
            .OrderBy(a => a.AppointmentTime)
            .Take(take)
            .ToListAsync();
    }
}
