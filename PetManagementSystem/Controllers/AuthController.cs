using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Services;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    [AllowAnonymous]
    [HttpGet("/User/Login")]
    public IActionResult UserLogin(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "AdminHome");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View("UserLogin", new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost("/User/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserLogin(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View("UserLogin", model);
        }

        var success = await authService.LoginAsync(model.Username, model.Password);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Kiểm tra tài khoản/mật khẩu, trạng thái user active hoặc kết nối cơ sở dữ liệu.");
            return View("UserLogin", model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("/Admin/Login")]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View("AdminLogin", new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost("/Admin/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View("AdminLogin", model);
        }

        var success = await authService.LoginAsync(model.Username, model.Password);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Kiểm tra tài khoản/mật khẩu, trạng thái user active hoặc kết nối cơ sở dữ liệu.");
            return View("AdminLogin", model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Dashboard", "Admin");
    }

    [AllowAnonymous]
    [HttpGet("/Auth/Login")]
    public IActionResult LoginAlias()
    {
        return RedirectToAction(nameof(UserLogin));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "AdminHome");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.RegisterAsync(model.Username, model.Password, model.FullName, model.Email, model.Phone);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(UserLogin));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return RedirectToAction(nameof(UserLogin));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutAdmin()
    {
        await authService.LogoutAsync();
        return RedirectToAction(nameof(AdminLogin));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
