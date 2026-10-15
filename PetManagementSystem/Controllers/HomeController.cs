using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;
using System.Security.Claims;

namespace PetManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _dbContext;

    public HomeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Services()
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

        return View("~/Views/Service/Index.cshtml", viewModel);
    }

    public IActionResult Booking(int? serviceId, int? packageId, int? servicePackageId)
    {
        return RedirectToAction("Book", "Appointment", new { serviceId, packageId, servicePackageId });
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Doctors()
    {
        // For now, return empty view; later can fetch from Veterinarians table
        return View();
    }

    public IActionResult Blog()
    {
        // For now, return empty view; later can fetch from BlogPosts table if exists
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
