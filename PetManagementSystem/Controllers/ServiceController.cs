using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;
using System.Security.Claims;

namespace PetManagementSystem.Controllers;

public class ServiceController : Controller
{
    private readonly AppDbContext _dbContext;

    public ServiceController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var services = await _dbContext.Services
            .Where(s => s.Status == 1)
            .AsNoTracking()
            .ToListAsync();

        var packages = await _dbContext.ServicePackages
            .Where(sp => sp.Status == 1)
            .Include(sp => sp.ServicePackageServices)
                .ThenInclude(sps => sps.Service)
            .AsNoTracking()
            .ToListAsync();

        List<Pet> userPets = new();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                userPets = await _dbContext.Pets.Where(p => p.UserId == userId).AsNoTracking().ToListAsync();
            }
        }
        ViewBag.UserPets = userPets;

        var viewModel = new ServiceIndexViewModel
        {
            Services = services,
            Packages = packages
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuyPackage(int packageId, int petId, string paymentMethod)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var pet = await _dbContext.Pets.FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);
        if (pet == null)
        {
            TempData["ErrorMessage"] = "Thú cưng không tồn tại hoặc không thuộc sở hữu của bạn.";
            return RedirectToAction(nameof(Index));
        }

        var package = await _dbContext.ServicePackages.FindAsync(packageId);
        if (package == null || package.Status != 1)
        {
            TempData["ErrorMessage"] = "Gói dịch vụ không tồn tại hoặc không khả dụng.";
            return RedirectToAction(nameof(Index));
        }

        var activePp = await _dbContext.PetPackages
            .FirstOrDefaultAsync(pp => pp.PetId == petId && pp.Status == 1 && pp.EndDate >= DateTime.Now);
        if (activePp != null)
        {
            TempData["ErrorMessage"] = $"Thú cưng {pet.Name} đang sở hữu một gói dịch vụ active khác!";
            return RedirectToAction(nameof(Index));
        }

        var petPackage = new PetPackage
        {
            PetId = petId,
            PackageId = packageId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(package.ValidityDays),
            Status = 1
        };

        _dbContext.PetPackages.Add(petPackage);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Đăng ký gói dịch vụ '{package.Name}' thành công cho thú cưng {pet.Name}!";
        return RedirectToAction("Details", "Pets", new { id = petId });
    }
}
