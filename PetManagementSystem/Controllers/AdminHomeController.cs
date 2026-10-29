using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Filters;

namespace PetManagementSystem.Controllers;

[Authorize]
[ServiceFilter(typeof(AdminAccessFilter))]
public class AdminHomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    public IActionResult AdminOnly()
    {
        return View();
    }

    [Authorize(Policy = "users.view")]
    public IActionResult UsersViewOnly()
    {
        return View();
    }
}
