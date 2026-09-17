using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;

namespace PetManagementSystem.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher<User> passwordHasher,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var user = await userRepository.GetUserWithRoleAndPermissionsAsync(username.Trim());
            if (user is null)
            {
                return false;
            }

            if (!string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return false;
            }

            var principal = await BuildClaimsPrincipalAsync(user);

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return false;
            }

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true
                });

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed due to system/database error for username: {Username}", username);
            return false;
        }
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(string username, string password, string fullName, string email, string phoneNumber)
    {
        try
        {
            var normalizedUsername = username.Trim();
            var normalizedEmail = email.Trim();

            var existingUser = await userRepository.GetByUsernameAsync(normalizedUsername);
            if (existingUser is not null)
            {
                return (false, "Username đã tồn tại.");
            }

            var customerRole = await roleRepository.GetByNameAsync("customer");
            if (customerRole is null)
            {
                return (false, "Không tìm thấy role mặc định 'customer'.");
            }

            var user = new User
            {
                RoleId = customerRole.Id,
                Username = normalizedUsername,
                FullName = fullName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = phoneNumber.Trim(),
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = passwordHasher.HashPassword(user, password);

            await userRepository.AddAsync(user);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Register failed due to system/database error for username: {Username}", username);
            return (false, "Không thể đăng ký do lỗi hệ thống hoặc cơ sở dữ liệu.");
        }
    }

    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public Task<ClaimsPrincipal> BuildClaimsPrincipalAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        };

        var permissions = user.Role?.RolePermissions
            .Where(rp => rp.Permission is not null && rp.Permission.IsActive)
            .Select(rp => rp.Permission!.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        claims.AddRange(permissions.Select(code => new Claim("permission", code)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(principal);
    }
}
