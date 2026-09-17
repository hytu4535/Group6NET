using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetManagementSystem.Controllers
{
    // Controller này chỉ dùng để điều hướng tới các trang giao diện (UI) quản trị.
    // Chưa gắn dữ liệu/database - sẽ bổ sung sau khi có model & DbContext.
    [Authorize]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            SetNav("Dashboard", "Dashboard", string.Empty);
            return View();
        }

        // Legacy pages (giữ tương thích)
        public IActionResult Customers()
        {
            SetNav("Quản lý Khách hàng", "Users", "Identity");
            return View();
        }

        public IActionResult Pets()
        {
            SetNav("Quản lý Thú cưng", "Pets", "PetCare");
            return View();
        }

        public IActionResult Employees()
        {
            SetNav("Quản lý Nhân viên", "Staff", "Identity");
            return View();
        }

        public IActionResult Services()
        {
            SetNav("Quản lý Gói dịch vụ", "Services", "ServiceMedical");
            return View();
        }

        public IActionResult Appointments()
        {
            SetNav("Quản lý Lịch hẹn", "Appointments", "ServiceMedical");
            return View();
        }

        public IActionResult Products()
        {
            SetNav("Quản lý Sản phẩm", "Products", "Commerce");
            return View();
        }

        public IActionResult Orders()
        {
            SetNav("Quản lý Đơn hàng", "Orders", "Commerce");
            return View();
        }

        public IActionResult Feedback()
        {
            SetNav("Phản hồi Khách hàng", "Feedbacks", "CustomerCare");
            return View();
        }

        // Identity
        public IActionResult Users() { SetNav("Users", "Users", "Identity"); return View(); }
        public IActionResult Roles() { SetNav("Roles", "Roles", "Identity"); return View(); }
        public IActionResult Permissions() { SetNav("Permissions", "Permissions", "Identity"); return View(); }
        public IActionResult RolePermissions() { SetNav("Role Permissions", "RolePermissions", "Identity"); return View(); }
        public IActionResult Staff() { SetNav("Staff", "Staff", "Identity"); return View(); }
        public IActionResult Veterinarians() { SetNav("Veterinarians", "Veterinarians", "Identity"); return View(); }

        // PetCare
        public IActionResult PetImages() { SetNav("Pet Images", "PetImages", "PetCare"); return View(); }
        public IActionResult Notifications() { SetNav("Notifications", "Notifications", "CustomerCare"); return View(); }
        public IActionResult Feedbacks() { SetNav("Feedbacks", "Feedbacks", "CustomerCare"); return View(); }
        public IActionResult Chats() { SetNav("Chat Conversations", "Chats", "CustomerCare"); return View(); }

        // Service & Medical
        public IActionResult ServicePackages() { SetNav("Service Packages", "ServicePackages", "ServiceMedical"); return View(); }
        public IActionResult PetPackages() { SetNav("Pet Packages", "PetPackages", "ServiceMedical"); return View(); }
        public IActionResult AppointmentServices() { SetNav("Appointment Services", "AppointmentServices", "ServiceMedical"); return View(); }
        public IActionResult PetHealthRecords() { SetNav("Pet Health Records", "PetHealthRecords", "ServiceMedical"); return View(); }
        public IActionResult VaccinationRecords() { SetNav("Vaccination Records", "VaccinationRecords", "ServiceMedical"); return View(); }
        public IActionResult MedicalPrescriptions() { SetNav("Medical Prescriptions", "MedicalPrescriptions", "ServiceMedical"); return View(); }

        // Commerce
        public IActionResult Suppliers() { SetNav("Suppliers", "Suppliers", "Commerce"); return View(); }
        public IActionResult ImportReceipts() { SetNav("Import Receipts", "ImportReceipts", "Commerce"); return View(); }
        public IActionResult ImportDetails() { SetNav("Import Details", "ImportDetails", "Commerce"); return View(); }
        public IActionResult Categories() { SetNav("Categories", "Categories", "Commerce"); return View(); }
        public IActionResult Carts() { SetNav("Carts", "Carts", "Commerce"); return View(); }
        public IActionResult CartItems() { SetNav("Cart Items", "CartItems", "Commerce"); return View(); }
        public IActionResult OrderItems() { SetNav("Order Items", "OrderItems", "Commerce"); return View(); }
        public IActionResult Payments() { SetNav("Payments", "Payments", "Commerce"); return View(); }
        public IActionResult Invoices() { SetNav("Invoices", "Invoices", "Commerce"); return View(); }

        private void SetNav(string title, string active, string activeParent)
        {
            ViewData["Title"] = title;
            ViewData["Active"] = active;
            ViewData["ActiveParent"] = activeParent;
        }
    }
}
