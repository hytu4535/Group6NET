using PetManagementSystem.Models;

namespace PetManagementSystem.Repositories;

/// <summary>Các truy vấn chỉ-đọc phục vụ trang tổng quan quản trị.</summary>
public interface IDashboardRepository
{
    Task<int> CountPetsAsync();
    Task<int> CountActiveCustomersAsync();
    Task<int> CountAppointmentsByStatusAsync(byte status);

    /// <summary>Lịch hẹn của ngày <paramref name="day"/> còn hiệu lực (chờ xác nhận hoặc đã xác nhận).</summary>
    Task<int> CountOpenAppointmentsOnAsync(DateTime day);

    Task<int> CountOrdersByStatusAsync(string status);
    Task<int> CountOrdersSinceAsync(DateTime from);
    Task<int> CountFeedbacksByStatusAsync(string status);

    /// <summary>Doanh thu theo tháng (12 phần tử) của năm, tính từ giao dịch thanh toán đã hoàn tất.</summary>
    Task<decimal[]> GetMonthlyRevenueAsync(int year);

    Task<List<Order>> GetRecentOrdersAsync(int take);
    Task<List<Appointment>> GetOpenAppointmentsOnAsync(DateTime day, int take);
}
