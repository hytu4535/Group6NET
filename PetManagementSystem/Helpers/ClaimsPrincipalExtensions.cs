using System.Security.Claims;
using PetManagementSystem.Models;
namespace PetManagementSystem.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    /// <summary>
    /// Kiểm tra quyền để ẩn/hiện nút trên giao diện (cùng logic với PermissionHandler:
    /// vai trò "admin" luôn có toàn quyền). Server vẫn là nơi chặn thật sự qua [Authorize(Policy = ...)].
    /// </summary>
    public static bool HasPermission(this ClaimsPrincipal principal, string permissionCode)
    {
        if (principal.IsInRole("admin"))
        {
            return true;
        }

        return principal.Claims.Any(x =>
            x.Type == "permission" &&
            string.Equals(x.Value, permissionCode, StringComparison.OrdinalIgnoreCase));
    }
}
