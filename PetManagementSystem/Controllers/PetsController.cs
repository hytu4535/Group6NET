using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

[Authorize]
public class PetsController(IPetService petService, AppDbContext dbContext) : Controller
{
    // GET: /Pets/Public/{token}
    [AllowAnonymous]
    [HttpGet("Pets/Public/{token}")]
    public async Task<IActionResult> PublicInfo(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return NotFound("Mã QR không hợp lệ.");
        }

        var pet = await petService.GetPetByTokenAsync(token);
        if (pet is null)
        {
            return NotFound("Không tìm thấy thông tin thú cưng hoặc mã QR không hợp lệ.");
        }

        return View(pet);
    }

    // GET: /Pets
    public async Task<IActionResult> Index(string? searchKeyword)
    {
        var pets = await petService.GetAllPetsAsync(searchKeyword);
        ViewData["SearchKeyword"] = searchKeyword;
        return View(pets);
    }

    // GET: /Pets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var pet = await petService.GetPetByIdAsync(id);
        if (pet is null) return NotFound();

        ViewBag.ActivePetPackage = await dbContext.PetPackages
            .Include(pp => pp.ServicePackage)
                .ThenInclude(sp => sp.ServicePackageServices)
                    .ThenInclude(sps => sps.Service)
            .FirstOrDefaultAsync(pp => pp.PetId == id && pp.Status == 1 && pp.EndDate >= DateTime.Now);

        ViewBag.HealthRecords = await dbContext.PetHealthRecords
            .Include(hr => hr.MedicalPrescriptions)
            .Include(hr => hr.Veterinarian).ThenInclude(v => v!.User)
            .Where(hr => hr.PetId == id)
            .OrderByDescending(hr => hr.VisitDate)
            .ToListAsync();

        ViewBag.VaccinationRecords = await dbContext.VaccinationRecords
            .Include(vr => vr.Veterinarian).ThenInclude(v => v!.User)
            .Where(vr => vr.PetId == id)
            .OrderByDescending(vr => vr.AdministeredDate)
            .ToListAsync();

        return View(pet);
    }

    // GET: /Pets/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Pets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pet pet)
    {
        if (!ModelState.IsValid) return View(pet);

        var (success, errorMessage) = await petService.CreatePetAsync(pet);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(pet);
        }

        TempData["SuccessMessage"] = "Thêm thú cưng thành công!";
        if (User.IsInRole("admin"))
        {
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("MyPets", "Account");
    }

    // GET: /Pets/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await petService.GetPetByIdAsync(id);
        if (pet is null) return NotFound();

        return View(pet);
    }

    // POST: /Pets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pet pet)
    {
        if (id != pet.Id) return BadRequest();
        if (!ModelState.IsValid) return View(pet);

        var (success, errorMessage) = await petService.UpdatePetAsync(pet);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            return View(pet);
        }

        TempData["SuccessMessage"] = "Cập nhật thông tin thú cưng thành công!";
        if (User.IsInRole("admin"))
        {
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("MyPets", "Account");
    }

    // POST: /Pets/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await petService.DeletePetAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
        }
        else
        {
            TempData["SuccessMessage"] = "Đã xóa thú cưng thành công!";
        }

        if (User.IsInRole("admin"))
        {
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("MyPets", "Account");
    }
}