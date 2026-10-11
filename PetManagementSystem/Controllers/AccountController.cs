using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PetManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _dbContext;

        public AccountController(IAuthService authService, IUserRepository userRepository, IDataProtectionProvider dataProtectionProvider, IPasswordHasher<User> passwordHasher, ILogger<AccountController> logger, AppDbContext dbContext)
        {
            _authService = authService;
            _userRepository = userRepository;
            _dataProtectionProvider = dataProtectionProvider;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _dbContext = dbContext;
        }

        private JsonResult JsonResponse(bool success, string message = "", object? data = null, IEnumerable<string>? errors = null)
        {
            return new JsonResult(new
            {
                success,
                message,
                data,
                errors = errors ?? Array.Empty<string>()
            });
        }

        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }

        [AllowAnonymous]
        [HttpGet("/Account/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost("/Account/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var success = await _authService.LoginAsync(model.Username, model.Password);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Kiểm tra tài khoản/mật khẩu, trạng thái user active hoặc kết nối cơ sở dữ liệu.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Đăng nhập thất bại.", null, GetModelStateErrors());
                }
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(true, "Đăng nhập thành công.", new { returnUrl });
                }
                return LocalRedirect(returnUrl);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Đăng nhập thành công.", new { redirectUrl = Url.Action("Index", "Home") });
            }
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet("/Account/Register")]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost("/Account/Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var result = await _authService.RegisterAsync(model.Username, model.Password, model.FullName, model.Email, model.Phone);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, result.ErrorMessage, null, GetModelStateErrors());
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Đăng ký thành công.", new { redirectUrl = Url.Action(nameof(Login)) });
            }
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpPost("/Account/Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Đăng xuất thành công.", new { redirectUrl = Url.Action(nameof(Login)) });
            }
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet("/Account/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost("/Account/ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                // To prevent email enumeration, we show the same message regardless of whether the email exists
                ModelState.AddModelError(string.Empty, "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(true, "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.", null);
                }
                return View(model);
            }

            // Generate a password reset token that expires in 1 hour
            var protector = _dataProtectionProvider.CreateProtector("PasswordReset");
            var tokenData = $"{user.Id}:{DateTime.UtcNow.AddHours(1):o}";
            var token = protector.Protect(tokenData);

            // In a real application, you would send an email with the reset link.
            // For this example, we'll just log the token and show a message.
            _logger.LogInformation("Password reset token for user {UserId}: {Token}", user.Id, token);

            // Normally, you would send an email here.
            // For simplicity, we'll just show a message that instructions have been sent.
            TempData["InfoMessage"] = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn.", null);
            }
            return RedirectToAction(nameof(ForgotPassword));
        }

        [AllowAnonymous]
        [HttpGet("/Account/ResetPassword")]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var protector = _dataProtectionProvider.CreateProtector("PasswordReset");
            try
            {
                var tokenData = protector.Unprotect(token);
                var parts = tokenData.Split(':');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out int userId) ||
                    !DateTime.TryParse(parts[1], out DateTime expiry) ||
                    expiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
                    return View(new ResetPasswordViewModel { Token = token });
                }

                var viewModel = new ResetPasswordViewModel { Token = token };
                return View(viewModel);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Token không hợp lệ.");
                return View(new ResetPasswordViewModel { Token = token });
            }
        }

        [AllowAnonymous]
        [HttpPost("/Account/ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var protector = _dataProtectionProvider.CreateProtector("PasswordReset");
            try
            {
                var tokenData = protector.Unprotect(model.Token);
                var parts = tokenData.Split(':');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out int userId) ||
                    !DateTime.TryParse(parts[1], out DateTime expiry) ||
                    expiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return JsonResponse(false, "Token không hợp lệ hoặc đã hết hạn.", null, GetModelStateErrors());
                    }
                    return View(model);
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return JsonResponse(false, "User not found.", null, GetModelStateErrors());
                    }
                    return View(model);
                }

                // Update password
                user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
                await _userRepository.UpdateAsync(user);

                TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi thành công. Vui lòng đăng nhập với mật khẩu mới.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(true, "Mật khẩu đã được đặt lại thành công.", new { redirectUrl = Url.Action(nameof(Login)) });
                }
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", model.Token);
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đặt lại mật khẩu. Vui lòng thử lại.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Đã xảy ra lỗi khi đặt lại mật khẩu.", null, GetModelStateErrors());
                }
                return View(model);
            }
        }

        [Authorize]
        [HttpGet("/Account/Profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Lấy hồ sơ thành công.", user);
            }
            return View(user);
        }

        [Authorize]
        [HttpGet("/Account/EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Lấy dữ liệu hồ sơ thành công.", model);
            }
            return View(model);
        }

        [Authorize]
        [HttpPost("/Account/EditProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            TempData["SuccessMessage"] = "Hồ sơ của bạn đã được cập nhật thành công.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Cập nhật hồ sơ thành công.", user);
            }
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet("/Account/ChangePassword")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost("/Account/ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Có lỗi trong dữ liệu nhập.", null, GetModelStateErrors());
                }
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return JsonResponse(false, "Mật khẩu hiện tại không đúng.", null, GetModelStateErrors());
                }
                return View(model);
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi thành công.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Mật khẩu đã được thay đổi thành công.", user);
            }
            return RedirectToAction(nameof(Profile));
        }

        // New actions for client portal
        [Authorize]
        [HttpGet("/Account/MyPets")]
        public async Task<IActionResult> MyPets()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var pets = await _dbContext.Pets
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var petIds = pets.Select(p => p.Id).ToList();
            var activePackages = await _dbContext.PetPackages
                .Include(pp => pp.ServicePackage)
                    .ThenInclude(sp => sp.ServicePackageServices)
                        .ThenInclude(sps => sps.Service)
                .Where(pp => petIds.Contains(pp.PetId) && pp.Status == 1 && pp.EndDate >= DateTime.Now)
                .AsNoTracking()
                .ToListAsync();

            var petImages = await _dbContext.PetImages
                .Where(pi => petIds.Contains(pi.PetId) && pi.IsAvatar == true)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.ActivePackages = activePackages;
            ViewBag.PetImages = petImages;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Danh sách thú cưng lấy thành công.", pets);
            }
            return View(pets);
        }

        [Authorize]
        [HttpGet("/Account/MyPackages")]
        public async Task<IActionResult> MyPackages()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userPetIds = await _dbContext.Pets.Where(p => p.UserId == userId).Select(p => p.Id).ToListAsync();

            var petPackages = await _dbContext.PetPackages
                .Include(pp => pp.ServicePackage)
                    .ThenInclude(sp => sp.ServicePackageServices)
                        .ThenInclude(sps => sps.Service)
                .Include(pp => pp.Pet)
                .Where(pp => userPetIds.Contains(pp.PetId))
                .OrderByDescending(pp => pp.StartDate)
                .ToListAsync();

            return View(petPackages);
        }

        [Authorize]
        [HttpGet("/Account/MyAppointments")]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var appointments = await _dbContext.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Danh sách lịch hẹn lấy thành công.", appointments);
            }
            return View(appointments);
        }

        [Authorize]
        [HttpGet("/Account/MyOrders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var orders = await _dbContext.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return JsonResponse(true, "Danh sách đơn hàng lấy thành công.", orders);
            }
            return View(orders);
        }
    }
}