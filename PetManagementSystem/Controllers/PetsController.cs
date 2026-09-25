using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using PetManagementSystem.Services;

namespace PetManagementSystem.Controllers;

[Authorize]
public class PetsController(IPetService petService) : Controller
{
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
        return RedirectToAction(nameof(Index));
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
        return RedirectToAction(nameof(Index));
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

        return RedirectToAction(nameof(Index));
    }
}