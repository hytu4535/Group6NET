using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels;

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
        private const long MaximumAvatarRequestBytes =
            RecognitionImageFileAttribute.MaximumFileSizeBytes + 64 * 1024;

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
            return View(new CreatePetViewModel());
        }

        // POST: /Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaximumAvatarRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaximumAvatarRequestBytes)]
        public async Task<IActionResult> Create(CreatePetViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var pet = new Pet
            {
                UserId = model.UserId,
                Name = model.Name,
                Species = model.Species,
                Breed = model.Breed,
                Gender = model.Gender,
                WeightKg = model.WeightKg
            };
            var (success, errorMessage) = await petService.CreatePetAsync(pet, model.AvatarFile);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
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

            return View(new EditPetViewModel
            {
                Id = pet.Id,
                UserId = pet.UserId,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                Gender = pet.Gender,
                WeightKg = pet.WeightKg,
                CurrentAvatarUrl = GetAvatarUrl(pet),
                GalleryImages = GetGalleryImages(pet)
            });
        }

        // POST: /Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaximumAvatarRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaximumAvatarRequestBytes)]
        public async Task<IActionResult> Edit(int id, EditPetViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                var existingPet = await petService.GetPetByIdAsync(id);
                if (existingPet is null) return NotFound();
                model.CurrentAvatarUrl = GetAvatarUrl(existingPet);
                model.GalleryImages = GetGalleryImages(existingPet);
                return View(model);
            }

            var pet = new Pet
            {
                Id = model.Id,
                UserId = model.UserId,
                Name = model.Name,
                Species = model.Species,
                Breed = model.Breed,
                Gender = model.Gender,
                WeightKg = model.WeightKg
            };
            var (success, errorMessage) = await petService.UpdatePetAsync(pet, model.AvatarFile);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                var existingPet = await petService.GetPetByIdAsync(id);
                if (existingPet is null) return NotFound();
                model.CurrentAvatarUrl = GetAvatarUrl(existingPet);
                model.GalleryImages = GetGalleryImages(existingPet);
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật thông tin thú cưng thành công!";
            if (User.IsInRole("admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("MyPets", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvatar(int petId, int imageId, string? returnUrl = null)
        {
            var (success, errorMessage) = await petService.SetPetAvatarAsync(petId, imageId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã cập nhật ảnh đại diện từ thư viện.";
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Edit), new { id = petId });
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


        private static string? GetAvatarUrl(Pet pet) => pet.PetImages
            .Where(image => image.IsAvatar)
            .OrderByDescending(image => image.Id)
            .Select(image => image.ImageUrl)
            .FirstOrDefault()
            ?? pet.PetImages
                .OrderByDescending(image => image.Id)
                .Select(image => image.ImageUrl)
                .FirstOrDefault();

        private static List<PetGalleryImageViewModel> GetGalleryImages(Pet pet) => pet.PetImages
            .OrderByDescending(image => image.Id)
            .Select(image => new PetGalleryImageViewModel
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                IsAvatar = image.IsAvatar
            })
            .ToList();
    }