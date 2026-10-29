using System.Security.Claims;
using PetManagementSystem.Models;

namespace PetManagementSystem.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<(bool Success, string ErrorMessage)> RegisterAsync(string username, string password, string fullName, string email, string phoneNumber);
    Task LogoutAsync();
    Task<ClaimsPrincipal> BuildClaimsPrincipalAsync(User user);
}
