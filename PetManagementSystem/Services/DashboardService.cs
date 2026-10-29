using PetManagementSystem.Helpers;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public class DashboardService(IDashboardRepository repository) : IDashboardService
{
    public async Task<DashboardViewModel> GetAsync()
    {
        var now = DateTime.Now;
        var today = now.Date;

        // Các truy vấn dùng chung một DbContext nên chạy tuần tự (DbContext không an toàn khi chạy song song).
        var model = new DashboardViewModel
        {
            RevenueYear = now.Year,
            PetCount = await repository.CountPetsAsync(),
            CustomerCount = await repository.CountActiveCustomersAsync(),
            PendingAppointments = await repository.CountAppointmentsByStatusAsync(1),
            TodayAppointmentCount = await repository.CountOpenAppointmentsOnAsync(today),
            PendingOrders = await repository.CountOrdersByStatusAsync(OrderStatuses.Pending),
            OrdersToday = await repository.CountOrdersSinceAsync(today),
            PendingFeedbacks = await repository.CountFeedbacksByStatusAsync("pending"),
            MonthlyRevenue = await repository.GetMonthlyRevenueAsync(now.Year)
        };

        model.RecentOrders = (await repository.GetRecentOrdersAsync(5))
            .Select(o => new DashboardOrderRow
            {
                Id = o.Id,
                CustomerName = o.ReceiverName.TrimToNull() ?? CommerceFormat.DisplayName(o.User),
                Status = o.Status,
                TotalAmount = o.TotalAmount
            }).ToList();

        model.TodayAppointments = (await repository.GetOpenAppointmentsOnAsync(today, 6))
            .Select(a => new DashboardAppointmentRow
            {
                Id = a.Id,
                ServiceText = a.AppointmentServices.Count > 0
                    ? string.Join(", ", a.AppointmentServices.Select(s => s.Service.Name))
                    : (a.Reason.TrimToNull() ?? "Lịch hẹn"),
                Time = a.AppointmentTime,
                PetName = a.Pet?.Name ?? "-",
                OwnerName = CommerceFormat.DisplayName(a.User),
                Status = a.Status
            }).ToList();

        return model;
    }
}
