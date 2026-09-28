using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers
{
    [Authorize]
    public class AdminController(
      AppDbContext context,
      IPetService petService,
      IFeedbackService feedbackService,
      IUserRepository userRepository) : Controller
    {
        // Bỏ dòng: private readonly AppDbContext _context = context;
        private readonly IPetService _petService = petService;
        private readonly IFeedbackService _feedbackService = feedbackService;
        private readonly IUserRepository _userRepository = userRepository;
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
        public async Task<IActionResult> UpdatePet(Pet pet)
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

            return RedirectToAction(nameof(Pets));
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

        // Service & Medical - 3 Trang yêu cầu chính
        public async Task<IActionResult> Services()
        {
            SetNav("Quản lý Dịch vụ", "Services", "ServiceMedical");
            var services = await context.Services.AsNoTracking().ToListAsync();
            return View(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveService(Service model, List<int>? selectedServiceIds)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Quản lý Dịch vụ", "Services", "ServiceMedical");
                var services = await context.Services.AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "serviceModal";
                return View("Services", services);
            }

            if (model.Id == 0)
            {
                model.CreatedAt = DateTime.UtcNow;
                context.Services.Add(model);
                TempData["Success"] = "Thêm dịch vụ thành công!";
            }
            else
            {
                var existing = await context.Services.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Description = model.Description;
                    existing.Price = model.Price;
                    existing.DurationMinutes = model.DurationMinutes;
                    existing.Status = model.Status;
                    TempData["Success"] = "Cập nhật dịch vụ thành công!";
                }
            }
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Services));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreateService(Service model, List<int>? selectedServiceIds) => SaveService(model, selectedServiceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditService(Service model, List<int>? selectedServiceIds) => SaveService(model, selectedServiceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditService(int id, Service model, List<int>? selectedServiceIds) => SaveService(model, selectedServiceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await context.Services.FindAsync(id);
            if (service != null)
            {
                context.Services.Remove(service);
                await context.SaveChangesAsync();
                TempData["Success"] = "Xóa dịch vụ thành công!";
            }
            return RedirectToAction(nameof(Services));
        }

        public async Task<IActionResult> ServicePackages()
        {
            SetNav("Quản lý Gói dịch vụ", "ServicePackages", "ServiceMedical");
            var packages = await context.ServicePackages
                .Include(sp => sp.ServicePackageServices)
                    .ThenInclude(sps => sps.Service)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.AllServices = await context.Services.AsNoTracking().ToListAsync();
            return View(packages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveServicePackage(ServicePackage model, List<int>? serviceIds)
        {
            serviceIds ??= new List<int>();

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Quản lý Gói dịch vụ", "ServicePackages", "ServiceMedical");
                var packages = await context.ServicePackages
                    .Include(sp => sp.ServicePackageServices)
                        .ThenInclude(sps => sps.Service)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.AllServices = await context.Services.AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "packageModal";
                return View("ServicePackages", packages);
            }

            if (model.Id == 0)
            {
                model.CreatedAt = DateTime.UtcNow;
                context.ServicePackages.Add(model);
                await context.SaveChangesAsync();

                foreach (var sId in serviceIds)
                {
                    context.ServicePackageServices.Add(new ServicePackageService
                    {
                        PackageId = model.Id,
                        ServiceId = sId
                    });
                }
                TempData["Success"] = "Thêm gói dịch vụ thành công!";
            }
            else
            {
                var existing = await context.ServicePackages
                    .Include(sp => sp.ServicePackageServices)
                    .FirstOrDefaultAsync(sp => sp.Id == model.Id);

                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Description = model.Description;
                    existing.Price = model.Price;
                    existing.DurationMinutes = model.DurationMinutes;
                    existing.ValidityDays = model.ValidityDays;
                    existing.Status = model.Status;

                    // Update mapping
                    context.ServicePackageServices.RemoveRange(existing.ServicePackageServices);
                    foreach (var sId in serviceIds)
                    {
                        context.ServicePackageServices.Add(new ServicePackageService
                        {
                            PackageId = existing.Id,
                            ServiceId = sId
                        });
                    }

                    TempData["Success"] = "Cập nhật gói dịch vụ thành công!";
                }
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(ServicePackages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreateServicePackage(ServicePackage model, List<int>? serviceIds) => SaveServicePackage(model, serviceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditServicePackage(ServicePackage model, List<int>? serviceIds) => SaveServicePackage(model, serviceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditServicePackage(int id, ServicePackage model, List<int>? serviceIds) => SaveServicePackage(model, serviceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServicePackage(int id)
        {
            var package = await context.ServicePackages.FindAsync(id);
            if (package != null)
            {
                context.ServicePackages.Remove(package);
                await context.SaveChangesAsync();
                TempData["Success"] = "Xóa gói dịch vụ thành công!";
            }
            return RedirectToAction(nameof(ServicePackages));
        }

        public async Task<IActionResult> PetPackages()
        {
            SetNav("Gói dịch vụ của thú cưng", "PetPackages", "ServiceMedical");
            var petPackages = await context.PetPackages
                .Include(pp => pp.Pet)
                    .ThenInclude(p => p!.User)
                .Include(pp => pp.ServicePackage)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
            var activePetIds = await context.PetPackages.AsNoTracking().Where(pp => pp.Status == 1).Select(pp => pp.PetId).ToListAsync();
            ViewBag.AvailablePets = await context.Pets.AsNoTracking().Where(p => !activePetIds.Contains(p.Id)).ToListAsync();
            ViewBag.ActivePetIds = activePetIds;
            ViewBag.ServicePackages = await context.ServicePackages.AsNoTracking().ToListAsync();
            return View(petPackages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePetPackage(PetPackage model)
        {
            bool hasActive = await context.PetPackages
                .AsNoTracking()
                .AnyAsync(pp => pp.PetId == model.PetId && pp.Status == 1 && (model.Id == 0 || pp.Id != model.Id));

            if (hasActive)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Thú cưng này đã có gói dịch vụ đang hoạt động!" });
                }
                ModelState.AddModelError("PetId", "Thú cưng này đã có gói dịch vụ đang hoạt động!");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Gói dịch vụ của thú cưng", "PetPackages", "ServiceMedical");
                var petPackages = await context.PetPackages
                    .Include(pp => pp.Pet)
                        .ThenInclude(p => p!.User)
                    .Include(pp => pp.ServicePackage)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
                var activePetIds = await context.PetPackages.AsNoTracking().Where(pp => pp.Status == 1).Select(pp => pp.PetId).ToListAsync();
                ViewBag.AvailablePets = await context.Pets.AsNoTracking().Where(p => !activePetIds.Contains(p.Id)).ToListAsync();
                ViewBag.ActivePetIds = activePetIds;
                ViewBag.ServicePackages = await context.ServicePackages.AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "petPackageModal";
                return View("PetPackages", petPackages);
            }

            if (model.Id == 0)
            {
                context.PetPackages.Add(model);
                TempData["Success"] = "Đăng ký gói dịch vụ cho thú cưng thành công!";
            }
            else
            {
                var existing = await context.PetPackages.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.PetId = model.PetId;
                    existing.PackageId = model.PackageId;
                    existing.StartDate = model.StartDate;
                    existing.EndDate = model.EndDate;
                    existing.Status = model.Status;
                    TempData["Success"] = "Cập nhật gói dịch vụ thú cưng thành công!";
                }
            }

            await context.SaveChangesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = TempData["Success"]?.ToString() });
            }
            return RedirectToAction(nameof(PetPackages));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreatePetPackage(PetPackage model) => SavePetPackage(model);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditPetPackage(PetPackage model) => SavePetPackage(model);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditPetPackage(int id, PetPackage model) => SavePetPackage(model);
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePetPackage(int id)
        {
            var pp = await context.PetPackages.FindAsync(id);
            if (pp != null)
            {
                context.PetPackages.Remove(pp);
                await context.SaveChangesAsync();
                TempData["Success"] = "Xóa gói dịch vụ thú cưng thành công!";
            }
            return RedirectToAction(nameof(PetPackages));
        }

        [HttpGet]
        public async Task<IActionResult> GetActivePetPackage(int petId)
        {
            var activePp = await context.PetPackages
                .Include(pp => pp.ServicePackage)
                .AsNoTracking()
                .FirstOrDefaultAsync(pp => pp.PetId == petId && pp.Status == 1);

            if (activePp == null)
            {
                return Json(new { success = false });
            }

            return Json(new {
                success = true,
                packageId = activePp.PackageId,
                packageName = activePp.ServicePackage?.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            SetNav("Quản lý Lịch hẹn", "Appointments", "ServiceMedical");
            var appointments = await context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p!.User)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v!.User)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
            ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
            ViewBag.ServicePackages = await context.ServicePackages.AsNoTracking().ToListAsync();
            ViewBag.Services = await context.Services.AsNoTracking().ToListAsync();
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppointment(Appointment model, int? ServicePackageId, List<int>? serviceIds)
        {
            serviceIds ??= new List<int>();

            bool isPetBusy = await context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.Id != model.Id && a.PetId == model.PetId && a.AppointmentDate == model.AppointmentDate && a.Status != 3);

            if (isPetBusy)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Thú cưng này đã có lịch hẹn vào khung giờ này!" });
                }
                ModelState.AddModelError("AppointmentDate", "Thú cưng này đã có lịch hẹn vào khung giờ này!");
            }

            bool isDoctorBusy = await context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.Id != model.Id && a.VetId == model.VetId && a.AppointmentDate == model.AppointmentDate && a.Status != 3 && model.VetId != null);

            if (isDoctorBusy)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Bác sĩ phụ trách đã có lịch khám vào khung giờ này!" });
                }
                ModelState.AddModelError("VetId", "Bác sĩ phụ trách đã có lịch khám vào khung giờ này!");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Quản lý Lịch hẹn", "Appointments", "ServiceMedical");
                var appointments = await context.Appointments
                    .Include(a => a.Pet)
                        .ThenInclude(p => p!.User)
                    .Include(a => a.Veterinarian)
                        .ThenInclude(v => v!.User)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(asv => asv.Service)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
                ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
                ViewBag.ServicePackages = await context.ServicePackages.AsNoTracking().ToListAsync();
                ViewBag.Services = await context.Services.AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "appointmentModal";
                return View("Appointments", appointments);
            }

            var pet = await context.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.PetId);
            if (pet != null)
            {
                model.UserId = pet.UserId;
            }

            if (model.Id == 0)
            {
                context.Appointments.Add(model);
                await context.SaveChangesAsync();
                TempData["Success"] = "Tạo lịch hẹn thành công!";
            }
            else
            {
                var existing = await context.Appointments
                    .Include(a => a.AppointmentServices)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);

                if (existing != null)
                {
                    existing.PetId = model.PetId;
                    existing.UserId = model.UserId;
                    existing.VetId = model.VetId;
                    existing.AppointmentDate = model.AppointmentDate;
                    existing.AppointmentTime = model.AppointmentTime;
                    existing.Reason = model.Reason;
                    existing.Status = model.Status;

                    context.AppointmentServices.RemoveRange(existing.AppointmentServices);
                    await context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật lịch hẹn thành công!";
                }
            }

            foreach (var sId in serviceIds)
            {
                var srv = await context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sId);
                decimal price = srv?.Price ?? 0;
                context.AppointmentServices.Add(new AppointmentService
                {
                    AppointmentId = model.Id,
                    ServiceId = sId,
                    PriceAtBooking = price
                });
            }
            await context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = TempData["Success"]?.ToString() });
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreateAppointment(Appointment model, int? ServicePackageId, List<int>? serviceIds) => SaveAppointment(model, ServicePackageId, serviceIds);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditAppointment(Appointment model, int? ServicePackageId, List<int>? serviceIds) => SaveAppointment(model, ServicePackageId, serviceIds);

        [HttpGet]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appt = await context.Appointments
                .Include(a => a.AppointmentServices)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null)
            {
                return Json(new { success = false });
            }

            int? packageId = null;
            var activePp = await context.PetPackages.AsNoTracking().FirstOrDefaultAsync(pp => pp.PetId == appt.PetId && pp.Status == 1);
            if (activePp != null)
            {
                packageId = activePp.PackageId;
            }

            var serviceIds = appt.AppointmentServices.Select(asv => asv.ServiceId).ToList();

            return Json(new {
                success = true,
                id = appt.Id,
                petId = appt.PetId,
                vetId = appt.VetId,
                appointmentDate = appt.AppointmentDate.ToString("yyyy-MM-dd"),
                appointmentTime = appt.AppointmentTime.ToString(@"hh\:mm"),
                reason = appt.Reason,
                status = appt.Status,
                packageId = packageId,
                serviceIds = serviceIds
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appt = await context.Appointments.FindAsync(id);
            if (appt != null)
            {
                context.Appointments.Remove(appt);
                await context.SaveChangesAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa lịch hẹn thành công!" });
                }
                TempData["Success"] = "Xóa lịch hẹn thành công!";
            }
            return RedirectToAction(nameof(Appointments));
        }

        [HttpGet]
        public async Task<IActionResult> GetServicePrice(int serviceId)
        {
            var service = await context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null)
            {
                return Json(new { success = false });
            }
            return Json(new { success = true, price = service.Price });
        }

        [HttpGet]
        public async Task<IActionResult> PetHealthRecords()
        {
            SetNav("Hồ sơ sức khỏe", "PetHealthRecords", "ServiceMedical");
            var healthRecords = await context.PetHealthRecords
                .Include(h => h.Pet)
                    .ThenInclude(p => p.User)
                .Include(h => h.Veterinarian)
                    .ThenInclude(v => v.User)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
            ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
            return View(healthRecords);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePetHealthRecord(PetHealthRecord model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Hồ sơ sức khỏe", "PetHealthRecords", "ServiceMedical");
                var healthRecords = await context.PetHealthRecords
                    .Include(h => h.Pet)
                        .ThenInclude(p => p.User)
                    .Include(h => h.Veterinarian)
                        .ThenInclude(v => v.User)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
                ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "healthRecordModal";
                return View("PetHealthRecords", healthRecords);
            }

            if (model.Id == 0)
            {
                context.PetHealthRecords.Add(model);
                TempData["Success"] = "Thêm hồ sơ sức khỏe thành công!";
            }
            else
            {
                var record = await context.PetHealthRecords.FindAsync(model.Id);
                if (record == null) return NotFound();

                record.PetId = model.PetId;
                record.VetId = model.VetId;
                record.AppointmentId = model.AppointmentId;
                record.VisitDate = model.VisitDate;
                record.WeightKg = model.WeightKg;
                record.Temperature = model.Temperature;
                record.Diagnosis = model.Diagnosis;
                record.Treatment = model.Treatment;

                context.PetHealthRecords.Update(record);
                TempData["Success"] = "Cập nhật hồ sơ sức khỏe thành công!";
            }

            await context.SaveChangesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = TempData["Success"]?.ToString() });
            }
            return RedirectToAction(nameof(PetHealthRecords));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreatePetHealthRecord(PetHealthRecord model) => SavePetHealthRecord(model);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditPetHealthRecord(PetHealthRecord model) => SavePetHealthRecord(model);

        [HttpGet]
        public async Task<IActionResult> CreatePetHealthRecord()
        {
            var pets = await context.Pets.AsNoTracking().ToListAsync();
            var vets = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
            return Json(new { success = true, pets, veterinarians = vets });
        }

        [HttpGet]
        public async Task<IActionResult> EditPetHealthRecord(int id) => await GetPetHealthRecordById(id);

        [HttpGet]
        public async Task<IActionResult> GetPetHealthRecordById(int id)
        {
            var hr = await context.PetHealthRecords.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
            if (hr == null)
            {
                return Json(new { success = false });
            }
            return Json(new {
                success = true,
                id = hr.Id,
                petId = hr.PetId,
                vetId = hr.VetId,
                weightKg = hr.WeightKg,
                temperature = hr.Temperature,
                diagnosis = hr.Diagnosis,
                treatment = hr.Treatment,
                visitDate = hr.VisitDate.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePetHealthRecord(int id)
        {
            var hr = await context.PetHealthRecords.FindAsync(id);
            if (hr != null)
            {
                context.PetHealthRecords.Remove(hr);
                await context.SaveChangesAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa hồ sơ sức khỏe thành công!" });
                }
                TempData["Success"] = "Xóa hồ sơ sức khỏe thành công!";
            }
            return RedirectToAction(nameof(PetHealthRecords));
        }

        [HttpGet]
        public async Task<IActionResult> VaccinationRecords()
        {
            SetNav("Lịch sử tiêm phòng", "VaccinationRecords", "ServiceMedical");
            var vaccinationRecords = await context.VaccinationRecords
                .Include(v => v.Pet)
                    .ThenInclude(p => p.User)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vt => vt.User)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
            ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
            return View(vaccinationRecords);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVaccinationRecord(VaccinationRecord model)
        {
            if (model.NextDueDate <= model.AdministeredDate)
            {
                ModelState.AddModelError("NextDueDate", "Ngày tiêm tiếp theo phải sau ngày tiêm hiện tại!");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Lịch sử tiêm phòng", "VaccinationRecords", "ServiceMedical");
                var vaccinationRecords = await context.VaccinationRecords
                    .Include(v => v.Pet)
                        .ThenInclude(p => p.User)
                    .Include(v => v.Veterinarian)
                        .ThenInclude(vt => vt.User)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.Pets = await context.Pets.AsNoTracking().ToListAsync();
                ViewBag.Veterinarians = await context.Veterinarians.Include(v => v.User).AsNoTracking().ToListAsync();
                ViewBag.ShowModal = "vaccinationRecordModal";
                return View("VaccinationRecords", vaccinationRecords);
            }

            if (model.Id == 0)
            {
                context.VaccinationRecords.Add(model);
                TempData["Success"] = "Thêm lịch sử tiêm phòng thành công!";
            }
            else
            {
                var existing = await context.VaccinationRecords.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.PetId = model.PetId;
                    existing.VaccineName = model.VaccineName;
                    existing.BatchNumber = model.BatchNumber;
                    existing.AdministeredDate = model.AdministeredDate;
                    existing.NextDueDate = model.NextDueDate;
                    existing.VetId = model.VetId;
                    TempData["Success"] = "Cập nhật lịch sử tiêm phòng thành công!";
                }
            }

            await context.SaveChangesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = TempData["Success"]?.ToString() });
            }
            return RedirectToAction(nameof(VaccinationRecords));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreateVaccinationRecord(VaccinationRecord model) => SaveVaccinationRecord(model);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditVaccinationRecord(VaccinationRecord model) => SaveVaccinationRecord(model);

        [HttpGet]
        public async Task<IActionResult> GetVaccinationRecordById(int id)
        {
            var vr = await context.VaccinationRecords.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (vr == null)
            {
                return Json(new { success = false });
            }
            return Json(new {
                success = true,
                id = vr.Id,
                petId = vr.PetId,
                vaccineName = vr.VaccineName,
                batchNumber = vr.BatchNumber,
                administeredDate = vr.AdministeredDate.ToString("yyyy-MM-dd"),
                nextDueDate = vr.NextDueDate.ToString("yyyy-MM-dd"),
                vetId = vr.VetId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVaccinationRecord(int id)
        {
            var vr = await context.VaccinationRecords.FindAsync(id);
            if (vr != null)
            {
                context.VaccinationRecords.Remove(vr);
                await context.SaveChangesAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa lịch sử tiêm phòng thành công!" });
                }
                TempData["Success"] = "Xóa lịch sử tiêm phòng thành công!";
            }
            return RedirectToAction(nameof(VaccinationRecords));
        }

        [HttpGet]
        public async Task<IActionResult> MedicalPrescriptions()
        {
            SetNav("Đơn thuốc", "MedicalPrescriptions", "ServiceMedical");
            var prescriptions = await context.MedicalPrescriptions
                .Include(m => m.PetHealthRecord)
                    .ThenInclude(r => r.Pet)
                        .ThenInclude(p => p.User)
                .Include(m => m.PetHealthRecord)
                    .ThenInclude(r => r.Veterinarian)
                        .ThenInclude(v => v.User)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.HealthRecords = await context.PetHealthRecords
                .Include(r => r.Pet)
                    .ThenInclude(p => p.User)
                .AsNoTracking()
                .ToListAsync();
            return View(prescriptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMedicalPrescription(MedicalPrescription model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(ModelState);
                }
                SetNav("Đơn thuốc", "MedicalPrescriptions", "ServiceMedical");
                var prescriptions = await context.MedicalPrescriptions
                    .Include(m => m.PetHealthRecord)
                        .ThenInclude(r => r.Pet)
                            .ThenInclude(p => p.User)
                    .Include(m => m.PetHealthRecord)
                        .ThenInclude(r => r.Veterinarian)
                            .ThenInclude(v => v.User)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.HealthRecords = await context.PetHealthRecords
                    .Include(r => r.Pet)
                        .ThenInclude(p => p.User)
                    .AsNoTracking()
                    .ToListAsync();
                ViewBag.ShowModal = "prescriptionModal";
                return View("MedicalPrescriptions", prescriptions);
            }

            if (model.Id == 0)
            {
                context.MedicalPrescriptions.Add(model);
                TempData["Success"] = "Thêm đơn thuốc thành công!";
            }
            else
            {
                var existing = await context.MedicalPrescriptions.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.RecordId = model.RecordId;
                    existing.MedicationName = model.MedicationName;
                    existing.Dosage = model.Dosage;
                    existing.Frequency = model.Frequency;
                    existing.DurationDays = model.DurationDays;
                    TempData["Success"] = "Cập nhật đơn thuốc thành công!";
                }
            }

            await context.SaveChangesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = TempData["Success"]?.ToString() });
            }
            return RedirectToAction(nameof(MedicalPrescriptions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> CreateMedicalPrescription(MedicalPrescription model) => SaveMedicalPrescription(model);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EditMedicalPrescription(MedicalPrescription model) => SaveMedicalPrescription(model);

        [HttpGet]
        public async Task<IActionResult> GetMedicalPrescriptionById(int id)
        {
            var mp = await context.MedicalPrescriptions.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (mp == null)
            {
                return Json(new { success = false });
            }
            return Json(new {
                success = true,
                id = mp.Id,
                recordId = mp.RecordId,
                medicationName = mp.MedicationName,
                dosage = mp.Dosage,
                frequency = mp.Frequency,
                durationDays = mp.DurationDays
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalPrescription(int id)
        {
            var mp = await context.MedicalPrescriptions.FindAsync(id);
            if (mp != null)
            {
                context.MedicalPrescriptions.Remove(mp);
                await context.SaveChangesAsync();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Xóa đơn thuốc thành công!" });
                }
                TempData["Success"] = "Xóa đơn thuốc thành công!";
            }
            return RedirectToAction(nameof(MedicalPrescriptions));
        }

        private void SetNav(string title, string active, string activeParent)
        {
            ViewData["Title"] = title;
            ViewData["Active"] = active;
            ViewData["ActiveParent"] = activeParent;
        }

    }
}
