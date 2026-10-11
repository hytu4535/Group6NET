using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;

namespace PetManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly PetManagementSystem.Data.AppDbContext _dbContext;

        public ProductController(PetManagementSystem.Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var products = _dbContext.Products.Where(p => p.Status == "active").ToList();
            return View(products);
        }
    }
}