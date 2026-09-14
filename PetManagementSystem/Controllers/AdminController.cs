using Microsoft.AspNetCore.Mvc;

namespace PetManagementSystem.Controllers
{
    // Controller này chỉ dùng để điều hướng tới các trang giao diện (UI) quản trị.
    // Chưa gắn dữ liệu/database - sẽ bổ sung sau khi có model & DbContext.
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Active"] = "Dashboard";
            return View();
        }

        public IActionResult Customers()
        {
            ViewData["Title"] = "Quản lý Khách hàng";
            ViewData["Active"] = "Customers";
            return View();
        }

        public IActionResult Pets()
        {
            ViewData["Title"] = "Quản lý Thú cưng";
            ViewData["Active"] = "Pets";
            return View();
        }

        public IActionResult Employees()
        {
            ViewData["Title"] = "Quản lý Nhân viên";
            ViewData["Active"] = "Employees";
            return View();
        }

        public IActionResult Services()
        {
            ViewData["Title"] = "Quản lý Gói dịch vụ";
            ViewData["Active"] = "Services";
            return View();
        }

        public IActionResult Appointments()
        {
            ViewData["Title"] = "Quản lý Lịch hẹn";
            ViewData["Active"] = "Appointments";
            return View();
        }

        public IActionResult Products()
        {
            ViewData["Title"] = "Quản lý Sản phẩm";
            ViewData["Active"] = "Products";
            return View();
        }

        public IActionResult Orders()
        {
            ViewData["Title"] = "Quản lý Đơn hàng";
            ViewData["Active"] = "Orders";
            return View();
        }

        public IActionResult Feedback()
        {
            ViewData["Title"] = "Phản hồi Khách hàng";
            ViewData["Active"] = "Feedback";
            return View();
        }
    }
}
