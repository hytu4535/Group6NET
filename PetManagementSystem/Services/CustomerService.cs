using Microsoft.AspNetCore.Mvc.Rendering;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public sealed class CustomerService(
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository) : ICustomerService
{
    public Task<PagedResult<CustomerListItemViewModel>> SearchAsync(
        string? search, string? status, string? sort, bool descending, int page, int pageSize) =>
        customerRepository.SearchAsync(search, status, sort, descending, page, pageSize);

    public Task<CustomerDetailViewModel?> GetDetailsAsync(int id) => customerRepository.GetDetailsAsync(id);

    public async Task<CustomerFormViewModel?> GetFormAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        return customer is null ? null : await PopulateUserOptionsAsync(CustomerFormViewModel.FromUser(customer));
    }

    public Task<CustomerFormViewModel> PrepareCreateFormAsync() =>
        PopulateUserOptionsAsync(new CustomerFormViewModel());

    public async Task<CustomerUserLookupViewModel?> GetUserLookupAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null || string.Equals(user.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var roleName = user.Role?.Name ?? string.Empty;
        return new CustomerUserLookupViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address ?? string.Empty,
            RoleName = roleName,
            IsCustomer = IsCustomerRole(roleName)
        };
    }

    public async Task<(bool Success, string ErrorMessage)> CreateAsync(CustomerFormViewModel model)
    {
        var role = await GetCustomerRoleAsync();
        if (role is null)
        {
            return (false, "Không tìm thấy role Customer/member trong hệ thống.");
        }

        var customer = await userRepository.GetByIdAsync(model.UserId);
        if (customer is null || string.Equals(customer.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Tài khoản đã chọn không tồn tại hoặc đã bị xóa.");
        }

        if (IsCustomerRole(customer.Role?.Name))
        {
            return (false, "Username này đã có trong danh sách khách hàng, không thể thêm trùng.");
        }

        if (IsProtectedRole(customer.Role?.Name))
        {
            return (false, "Không thể chuyển tài khoản nhân viên, bác sĩ hoặc quản trị viên thành khách hàng.");
        }

        customer.RoleId = role.Id;
        customer.Status = "active";
        customer.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(customer);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateAsync(CustomerFormViewModel model)
    {
        var customer = await userRepository.GetByIdAsync(model.UserId);
        if (customer is null)
        {
            return (false, "Không tìm thấy khách hàng.");
        }

        if (!IsCustomerRole(customer.Role?.Name))
        {
            return (false, "Tài khoản này không còn thuộc role Customer/member.");
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> ToggleStatusAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer is null)
        {
            return (false, "Không tìm thấy khách hàng.");
        }

        customer.Status = string.Equals(customer.Status, "active", StringComparison.OrdinalIgnoreCase)
            ? "inactive"
            : "active";
        customer.UpdatedAt = DateTime.UtcNow;
        await customerRepository.UpdateAsync(customer);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer is null)
        {
            return (false, "Không tìm thấy khách hàng.");
        }

        if (await userRepository.HasRelatedDataAsync(id))
        {
            customer.Status = "deleted";
            customer.UpdatedAt = DateTime.UtcNow;
            await userRepository.UpdateAsync(customer);
            return (true, "Tài khoản đã phát sinh dữ liệu nên đã được xóa mềm và ẩn khỏi danh sách.");
        }

        await userRepository.DeleteHardAsync(customer);
        return (true, string.Empty);
    }

    private async Task<Role?> GetCustomerRoleAsync() =>
        await roleRepository.GetByNameAsync("member") ?? await roleRepository.GetByNameAsync("Customer");

    private async Task<CustomerFormViewModel> PopulateUserOptionsAsync(CustomerFormViewModel model)
    {
        var users = await userRepository.GetAllWithRoleAsync(includeInactive: true);
        model.UserOptions = users
            .Where(user => !string.Equals(user.Status, "deleted", StringComparison.OrdinalIgnoreCase))
            .Select(user => new SelectListItem
            {
                Value = user.Id.ToString(),
                Text = $"{user.Username} - {user.FullName}"
            });
        return model;
    }

    private static bool IsCustomerRole(string? roleName) =>
        string.Equals(roleName, "member", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(roleName, "customer", StringComparison.OrdinalIgnoreCase);

    private static bool IsProtectedRole(string? roleName) =>
        string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(roleName, "staff", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(roleName, "veterinarian", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(roleName, "vet", StringComparison.OrdinalIgnoreCase);
}
