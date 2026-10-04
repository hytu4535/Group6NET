using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Data;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels.Appointment;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PetManagementSystem.Controllers;

[Authorize]
public class AppointmentController(
    IAppointmentManagerService appointmentService,
    AppDbContext dbContext) : Controller
{
    private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("/Appointment/Book")]
    public async Task<IActionResult> Book(int? serviceId, int? packageId, int? servicePackageId)
    {
        var userId = GetCurrentUserId();
        ViewBag.Pets = await appointmentService.GetPetsByUserAsync(userId);
        ViewBag.Services = await appointmentService.GetActiveServicesAsync();

        var model = new BookingInputModel();
        if (serviceId.HasValue)
        {
            model.ServiceIds.Add(serviceId.Value);
        }
        int? pkgId = packageId ?? servicePackageId;
        if (pkgId.HasValue)
        {
            model.ServicePackageId = pkgId.Value;
            var petPackage = await dbContext.PetPackages
                .FirstOrDefaultAsync(pp => pp.PackageId == pkgId.Value && pp.Status == 1);
            if (petPackage != null)
            {
                model.PetId = petPackage.PetId;
            }
        }
        return View(model);
    }

    [HttpGet("/Appointment/Create")]
    public IActionResult Create(int? serviceId, int? packageId, int? servicePackageId)
    {
        return RedirectToAction(nameof(Book), new { serviceId, packageId, servicePackageId });
    }

    [HttpPost("/Appointment/Book")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookingInputModel model)
    {
        var userId = GetCurrentUserId();
        if (!ModelState.IsValid)
        {
            ViewBag.Pets = await appointmentService.GetPetsByUserAsync(userId);
            ViewBag.Services = await appointmentService.GetActiveServicesAsync();
            return View(model);
        }

        var result = await appointmentService.CreateAppointmentAsync(userId, model);
        if (!result.Success)
        {
            ModelState.AddModelError(result.Field ?? string.Empty, result.Error ?? "Không thể đặt lịch hẹn.");
            ViewBag.Pets = await appointmentService.GetPetsByUserAsync(userId);
            ViewBag.Services = await appointmentService.GetActiveServicesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Đặt lịch hẹn thành công! Đang chờ cửa hàng xác nhận.";
        return RedirectToAction(nameof(MyAppointments));
    }

    [HttpGet("/Appointment/GetActivePetPackage")]
    public async Task<IActionResult> GetActivePetPackage(int petId)
    {
        var userId = GetCurrentUserId();
        var pet = await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
        if (pet == null) return Json(new { hasPackage = false });

        var activePps = await dbContext.PetPackages
            .Include(pp => pp.ServicePackage)
                .ThenInclude(sp => sp.ServicePackageServices)
            .Where(pp => pp.PetId == petId && pp.Status == 1 && pp.EndDate >= DateTime.Now)
            .ToListAsync();

        if (!activePps.Any())
        {
            return Json(new { hasPackage = false });
        }

        var packages = activePps.Select(pp => new {
            packageId = pp.PackageId,
            packageName = pp.ServicePackage?.Name ?? "Gói dịch vụ",
            serviceIds = pp.ServicePackage?.ServicePackageServices?.Select(sps => sps.ServiceId).ToList() ?? new List<int>()
        }).ToList();

        return Json(new {
            hasPackage = true,
            packageId = activePps.First().PackageId,
            packageName = activePps.First().ServicePackage?.Name,
            serviceIds = activePps.First().ServicePackage?.ServicePackageServices?.Select(sps => sps.ServiceId).ToList() ?? new List<int>(),
            packages = packages
        });
    }

    [HttpGet("/Appointment/MyAppointments")]
    public async Task<IActionResult> MyAppointments()
    {
        var userId = GetCurrentUserId();
        var appointments = await appointmentService.GetAppointmentsByUserAsync(userId);
        return View("~/Views/Account/MyAppointments.cshtml", appointments);
    }

    [HttpGet("/Appointment/Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        var appointment = await appointmentService.GetAppointmentByIdAsync(id, userId);
        if (appointment == null) return NotFound();

        var healthRecord = await dbContext.PetHealthRecords
            .Include(hr => hr.MedicalPrescriptions)
            .Include(hr => hr.Veterinarian).ThenInclude(v => v!.User)
            .FirstOrDefaultAsync(hr => hr.AppointmentId == id);

        var vaccinationRecords = await dbContext.VaccinationRecords
            .Include(vr => vr.Veterinarian).ThenInclude(v => v!.User)
            .Where(vr => vr.PetId == appointment.PetId)
            .ToListAsync();

        ViewBag.HealthRecord = healthRecord;
        ViewBag.VaccinationRecords = vaccinationRecords;

        return View(appointment);
    }

    [HttpPost("/Appointment/Cancel/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancelReason)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(cancelReason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy lịch hẹn.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var result = await appointmentService.CancelAppointmentAsync(userId, id, cancelReason, "User");
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error;
        }
        else
        {
            TempData["SuccessMessage"] = "Hủy lịch hẹn thành công.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
