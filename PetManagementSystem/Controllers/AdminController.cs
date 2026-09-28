using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers
{
    // Controller này chỉ dùng để điều hướng tới các trang giao diện (UI) quản trị.
    // Chưa gắn dữ liệu/database - sẽ bổ sung sau khi có model & DbContext.
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IPetService _petService;
        private readonly IFeedbackService _feedbackService;
        private readonly IUserRepository _userRepository;

        public AdminController(
            IPetService petService,
            IFeedbackService feedbackService,
            IUserRepository userRepository)
        {
            _petService = petService;
            _feedbackService = feedbackService;
            _userRepository = userRepository;
        }
        public IActionResult Dashboard()
        {
            SetNav("Dashboard", "Dashboard", string.Empty);
            return View();
        }

        public async Task<IActionResult> Pets(string? searchKeyword, int page = 1, int pageSize = 10)
        {
            SetNav("Quản lý Thú cưng", "Pets", "PetCare");
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var pagedPets = await _petService.GetPagedPetsAsync(searchKeyword, page, pageSize);
            var users = await _userRepository.GetAllWithRoleAsync();
            ViewData["SearchKeyword"] = searchKeyword;
            ViewData["PetOwners"] = users;
            ViewData["Page"] = pagedPets.Page;
            ViewData["PageSize"] = pagedPets.PageSize;
            ViewData["TotalItems"] = pagedPets.TotalItems;
            return View(pagedPets.Items);
        }

        // POST: /Admin/CreatePet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePet(Pet pet)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _petService.CreatePetAsync(pet);
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm thú cưng mới thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ!";
            }

            return RedirectToAction(nameof(Pets));
        }

        // POST: /Admin/UpdatePet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePet(
            Pet pet,
            string? searchKeyword,
            int page = 1,
            int pageSize = 10)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _petService.UpdatePetAsync(pet);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thú cưng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }

            else
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ!";
            }

            return RedirectToAction(nameof(Pets), new { searchKeyword, page, pageSize });
        }

        // POST: /Admin/DeletePet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int id, string? searchKeyword, int page = 1, int pageSize = 10)
        {
            var (success, errorMessage) = await _petService.DeletePetAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Xóa thú cưng thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction(nameof(Pets), new { searchKeyword, page, pageSize });
        }


        public async Task<IActionResult> Feedbacks(int page = 1, int pageSize = 10)
        {
            SetNav("Phản hồi Khách hàng", "Feedbacks", "CustomerCare");
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var pagedFeedbacks = await _feedbackService.GetPagedFeedbacksAsync(page, pageSize);
            ViewData["Page"] = pagedFeedbacks.Page;
            ViewData["PageSize"] = pagedFeedbacks.PageSize;
            ViewData["TotalItems"] = pagedFeedbacks.TotalItems;
            return View(pagedFeedbacks.Items);
        }

        // Alias cho Feedback (giữ tương thích)
        public async Task<IActionResult> Feedback()
        {
            return RedirectToAction(nameof(Feedbacks));
        }

        // POST: /Admin/UpdateFeedbackStatus (Admin đổi trạng thái 'pending' <-> 'resolved')
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, string status, int page = 1, int pageSize = 10)
        {
            var (success, errorMessage) = await _feedbackService.UpdateStatusAsync(id, status);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái phản hồi thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction(nameof(Feedbacks), new { page, pageSize });
        }


        public IActionResult Customers()
        {
            SetNav("Quản lý Khách hàng", "Users", "Identity");
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

        // Identity
        public IActionResult Users() { SetNav("Users", "Users", "Identity"); return View(); }
        public IActionResult Roles() { SetNav("Roles", "Roles", "Identity"); return View(); }
        public IActionResult Permissions() { SetNav("Permissions", "Permissions", "Identity"); return View(); }
        public IActionResult RolePermissions() { SetNav("Role Permissions", "RolePermissions", "Identity"); return View(); }
        public IActionResult Staff() { SetNav("Staff", "Staff", "Identity"); return View(); }
        public IActionResult Veterinarians() { SetNav("Veterinarians", "Veterinarians", "Identity"); return View(); }

        // PetCare
        public IActionResult PetImages() { SetNav("Pet Images", "PetImages", "PetCare"); return View(); }

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
