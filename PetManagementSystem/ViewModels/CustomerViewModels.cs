using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetManagementSystem.Models;

namespace PetManagementSystem.ViewModels;

public sealed class CustomerListItemViewModel
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int PetCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CustomerUserLookupViewModel
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public bool IsCustomer { get; init; }
}

public sealed class CustomerDetailViewModel
{
    public User Customer { get; init; } = new();
    public IReadOnlyList<Pet> Pets { get; init; } = [];
    public IReadOnlyList<PetManagementSystem.Models.Appointment> Appointments { get; init; } = [];
    public IReadOnlyList<Order> Orders { get; init; } = [];
}

public sealed class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn tài khoản.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^(0|\+84)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại phải có 10 số và đúng định dạng Việt Nam.")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public IEnumerable<SelectListItem> UserOptions { get; set; } = [];

    public static CustomerFormViewModel FromUser(User user) => new()
    {
        Id = user.Id,
        UserId = user.Id,
        FullName = user.FullName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address
    };
}
