namespace PetManagementSystem.ViewModels;

public class DashboardOrderRow
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class DashboardAppointmentRow
{
    public int Id { get; set; }
    public string ServiceText { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public byte Status { get; set; }
}

/// <summary>Số liệu tổng quan trang quản trị, lấy trực tiếp từ cơ sở dữ liệu.</summary>
public class DashboardViewModel
{
    public int PetCount { get; set; }
    public int CustomerCount { get; set; }
    public int PendingAppointments { get; set; }
    public int TodayAppointmentCount { get; set; }
    public int PendingOrders { get; set; }
    public int OrdersToday { get; set; }
    public int PendingFeedbacks { get; set; }

    public int RevenueYear { get; set; }

    /// <summary>Doanh thu 12 tháng (index 0 = tháng 1) từ các giao dịch thanh toán đã hoàn tất.</summary>
    public decimal[] MonthlyRevenue { get; set; } = new decimal[12];
    public decimal YearRevenue => MonthlyRevenue.Sum();

    public List<DashboardOrderRow> RecentOrders { get; set; } = [];
    public List<DashboardAppointmentRow> TodayAppointments { get; set; } = [];
}
