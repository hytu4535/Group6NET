using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;

namespace PetManagementSystem.Controllers
{
    public class ServiceController : Controller
    {
        private readonly PetManagementSystem.Data.AppDbContext _dbContext;

        public ServiceController(PetManagementSystem.Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var services = _dbContext.Services.Where(s => s.Status == 1).ToList();
            return View(services);
        }
    }
}