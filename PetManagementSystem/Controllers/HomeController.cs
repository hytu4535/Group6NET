using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;

namespace PetManagementSystem.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult Booking()
    {
        return View();
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