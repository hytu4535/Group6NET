using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.ViewModels.Identity;

namespace PetManagementSystem.Controllers;

[Authorize]
public class IdentityController(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IStaffRepository staffRepository,
    IVeterinarianRepository veterinarianRepository,
    IPasswordHasher<User> passwordHasher) : Controller
{
    [Authorize(Policy = "users.view")]
    public async Task<IActionResult> Users()
    {
        SetNav("Quản lý người dùng", "Users", "Identity");
        var users = await userRepository.GetAllWithRoleAsync();
        return View(users);
    }

    [Authorize(Policy = "users.create")]
    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        SetNav("Tạo người dùng", "Users", "Identity");
        var vm = new UserUpsertViewModel
        {
            RoleOptions = await GetRoleOptionsAsync()
        };
        return View(vm);
    }

    [Authorize(Policy = "users.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserUpsertViewModel model)
    {
        SetNav("Tạo người dùng", "Users", "Identity");
        model.RoleOptions = await GetRoleOptionsAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await userRepository.ExistsUsernameAsync(model.Username.Trim()))
        {
            ModelState.AddModelError(nameof(model.Username), "Tên tài khoản đã tồn tại");
            return View(model);
        }

        if (await userRepository.ExistsEmailAsync(model.Email.Trim()))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại");
            return View(model);
        }

        var user = new User
        {
            RoleId = model.RoleId,
            Username = model.Username.Trim(),
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Address = model.Address?.Trim(),
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, model.Password!);

        await userRepository.AddAsync(user);
        TempData["Success"] = "Tạo người dùng thành công";
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Policy = "users.update")]
    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        SetNav("Cập nhật người dùng", "Users", "Identity");
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var vm = new UserUpsertViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address,
            RoleId = user.RoleId,
            Status = user.Status,
            RoleOptions = await GetRoleOptionsAsync()
        };

        return View(vm);
    }

    [Authorize(Policy = "users.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserUpsertViewModel model)
    {
        SetNav("Cập nhật người dùng", "Users", "Identity");
        model.RoleOptions = await GetRoleOptionsAsync();

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            return View(model);
        }

        var user = await userRepository.GetByIdAsync(model.Id.Value);
        if (user is null)
        {
            return NotFound();
        }

        if (!string.Equals(user.Username, model.Username, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.Username), "Không được phép đổi tên tài khoản");
            return View(model);
        }

        if (await userRepository.ExistsEmailAsync(model.Email.Trim(), user.Id))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại");
            return View(model);
        }

        user.RoleId = model.RoleId;
        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber.Trim();
        user.Address = model.Address?.Trim();
        user.Status = model.Status;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        }

        await userRepository.UpdateAsync(user);
        TempData["Success"] = "Cập nhật người dùng thành công";
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Policy = "roles.view")]
    public async Task<IActionResult> Roles()
    {
        SetNav("Quản lý vai trò", "Roles", "Identity");
        var roles = await roleRepository.GetAllAsync();
        return View(roles);
    }

    [Authorize(Policy = "roles.update")]
    [HttpGet]
    public async Task<IActionResult> CreateRole()
    {
        SetNav("Tạo vai trò", "Roles", "Identity");
        return View(await BuildRoleVmAsync(new RoleUpsertViewModel()));
    }

    [Authorize(Policy = "roles.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(RoleUpsertViewModel model)
    {
        SetNav("Tạo vai trò", "Roles", "Identity");
        model = await BuildRoleVmAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await roleRepository.ExistsNameAsync(model.Name.Trim()))
        {
            ModelState.AddModelError(nameof(model.Name), "Tên vai trò đã tồn tại");
            return View(model);
        }

        var role = new Role
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        await roleRepository.AddAsync(role, []);
        TempData["Success"] = "Tạo vai trò thành công";
        return RedirectToAction(nameof(Roles));
    }

    [Authorize(Policy = "roles.update")]
    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        SetNav("Cập nhật vai trò", "Roles", "Identity");
        var role = await roleRepository.GetByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var vm = new RoleUpsertViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Status = role.Status
        };

        return View(vm);
    }

    [Authorize(Policy = "roles.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(RoleUpsertViewModel model)
    {
        SetNav("Cập nhật vai trò", "Roles", "Identity");

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            return View(model);
        }

        var role = await roleRepository.GetRoleWithPermissionsAsync(model.Id.Value);
        if (role is null)
        {
            return NotFound();
        }

        if (await roleRepository.ExistsNameAsync(model.Name.Trim(), role.Id))
        {
            ModelState.AddModelError(nameof(model.Name), "Tên vai trò đã tồn tại");
            return View(model);
        }

        role.Name = model.Name.Trim();
        role.Description = model.Description?.Trim();
        role.UpdatedAt = DateTime.UtcNow;

        await roleRepository.UpdateAsync(role, role.RolePermissions.Select(x => x.PermissionId));
        TempData["Success"] = "Cập nhật vai trò thành công";
        return RedirectToAction(nameof(Roles));
    }

    [Authorize(Policy = "roles.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRoleStatus(int id)
    {
        var role = await roleRepository.GetRoleWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        role.Status = string.Equals(role.Status, "active", StringComparison.OrdinalIgnoreCase)
            ? "inactive"
            : "active";
        role.UpdatedAt = DateTime.UtcNow;

        await roleRepository.UpdateAsync(role, role.RolePermissions.Select(x => x.PermissionId));
        TempData["Success"] = role.Status == "active"
            ? "Đã kích hoạt vai trò"
            : "Đã khóa vai trò";
        return RedirectToAction(nameof(Roles));
    }

    [Authorize(Policy = "permissions.view")]
    public async Task<IActionResult> Permissions()
    {
        SetNav("Phân quyền", "Permissions", "Identity");
        var roles = await roleRepository.GetAllAsync();
        return View("~/Views/Admin/Permissions.cshtml", roles);
    }

    [Authorize(Policy = "permissions.update")]
    [HttpGet]
    public async Task<IActionResult> RolePermissions(int id)
    {
        SetNav("Phân quyền theo vai trò", "Permissions", "Identity");

        var role = await roleRepository.GetRoleWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var permissions = await EnsureCrudPermissionsAsync();
        var model = BuildRolePermissionsMatrix(role, permissions);
        return View("~/Views/Admin/RolePermissions.cshtml", model);
    }

    [Authorize(Policy = "permissions.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolePermissions(RolePermissionsViewModel model)
    {
        SetNav("Phân quyền theo vai trò", "Permissions", "Identity");

        var role = await roleRepository.GetRoleWithPermissionsAsync(model.RoleId);
        if (role is null)
        {
            return NotFound();
        }

        var selectedIds = model.SelectedPermissionIds ?? [];

        role.UpdatedAt = DateTime.UtcNow;
        await roleRepository.UpdateAsync(role, selectedIds);
        TempData["Success"] = "Cập nhật phân quyền thành công";
        return RedirectToAction(nameof(Permissions));
    }

    [Authorize(Policy = "permissions.update")]
    [HttpGet]
    public IActionResult CreatePermission()
    {
        SetNav("Tạo quyền", "Permissions", "Identity");
        return View(new PermissionUpsertViewModel());
    }

    [Authorize(Policy = "permissions.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePermission(PermissionUpsertViewModel model)
    {
        SetNav("Tạo quyền", "Permissions", "Identity");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await permissionRepository.ExistsCodeAsync(model.Code.Trim()))
        {
            ModelState.AddModelError(nameof(model.Code), "Mã quyền đã tồn tại");
            return View(model);
        }

        var permission = new Permission
        {
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            Module = model.Module.Trim(),
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await permissionRepository.AddAsync(permission);
        TempData["Success"] = "Tạo quyền thành công";
        return RedirectToAction(nameof(Permissions));
    }

    [Authorize(Policy = "permissions.update")]
    [HttpGet]
    public async Task<IActionResult> EditPermission(int id)
    {
        SetNav("Cập nhật quyền", "Permissions", "Identity");
        var permission = await permissionRepository.GetByIdAsync(id);
        if (permission is null)
        {
            return NotFound();
        }

        var vm = new PermissionUpsertViewModel
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            Module = permission.Module ?? string.Empty,
            IsActive = permission.IsActive
        };

        return View(vm);
    }

    [Authorize(Policy = "permissions.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPermission(PermissionUpsertViewModel model)
    {
        SetNav("Cập nhật quyền", "Permissions", "Identity");

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            return View(model);
        }

        var permission = await permissionRepository.GetByIdAsync(model.Id.Value);
        if (permission is null)
        {
            return NotFound();
        }

        if (await permissionRepository.ExistsCodeAsync(model.Code.Trim(), permission.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Mã quyền đã tồn tại");
            return View(model);
        }

        permission.Code = model.Code.Trim();
        permission.Name = model.Name.Trim();
        permission.Description = model.Description?.Trim();
        permission.Module = model.Module.Trim();
        permission.IsActive = model.IsActive;
        permission.UpdatedAt = DateTime.UtcNow;

        await permissionRepository.UpdateAsync(permission);
        TempData["Success"] = "Cập nhật quyền thành công";
        return RedirectToAction(nameof(Permissions));
    }

    [Authorize(Policy = "staff.view")]
    public async Task<IActionResult> Staff()
    {
        SetNav("Quản lý nhân viên", "Staff", "Identity");
        var staffList = await staffRepository.GetAllWithUserAsync();
        return View(staffList);
    }

    [Authorize(Policy = "staff.update")]
    [HttpGet]
    public async Task<IActionResult> CreateStaff()
    {
        SetNav("Tạo nhân viên", "Staff", "Identity");
        return View(await BuildStaffVmAsync(new StaffUpsertViewModel()));
    }

    [Authorize(Policy = "staff.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStaff(StaffUpsertViewModel model)
    {
        SetNav("Tạo nhân viên", "Staff", "Identity");
        model = await BuildStaffVmAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await staffRepository.ExistsByUserIdAsync(model.UserId))
        {
            ModelState.AddModelError(nameof(model.UserId), "User này đã được gán staff");
            return View(model);
        }

        var staff = new Staff
        {
            UserId = model.UserId,
            Position = model.Position?.Trim(),
            HireDate = model.HireDate,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        await staffRepository.AddAsync(staff);
        TempData["Success"] = "Tạo staff thành công";
        return RedirectToAction(nameof(Staff));
    }

    [Authorize(Policy = "staff.update")]
    [HttpGet]
    public async Task<IActionResult> EditStaff(int id)
    {
        SetNav("Cập nhật nhân viên", "Staff", "Identity");
        var staff = await staffRepository.GetByIdAsync(id);
        if (staff is null)
        {
            return NotFound();
        }

        var vm = new StaffUpsertViewModel
        {
            Id = staff.Id,
            UserId = staff.UserId,
            Position = staff.Position,
            HireDate = staff.HireDate,
            Status = staff.Status
        };

        return View(await BuildStaffVmAsync(vm));
    }

    [Authorize(Policy = "staff.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStaff(StaffUpsertViewModel model)
    {
        SetNav("Cập nhật nhân viên", "Staff", "Identity");
        model = await BuildStaffVmAsync(model);

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            return View(model);
        }

        var staff = await staffRepository.GetByIdAsync(model.Id.Value);
        if (staff is null)
        {
            return NotFound();
        }

        if (await staffRepository.ExistsByUserIdAsync(model.UserId, staff.Id))
        {
            ModelState.AddModelError(nameof(model.UserId), "User này đã được gán staff");
            return View(model);
        }

        staff.UserId = model.UserId;
        staff.Position = model.Position?.Trim();
        staff.HireDate = model.HireDate;
        staff.Status = model.Status;
        staff.UpdatedAt = DateTime.UtcNow;

        await staffRepository.UpdateAsync(staff);
        TempData["Success"] = "Cập nhật staff thành công";
        return RedirectToAction(nameof(Staff));
    }

    [Authorize(Policy = "veterinarians.view")]
    public async Task<IActionResult> Veterinarians()
    {
        SetNav("Quản lý bác sĩ thú y", "Veterinarians", "Identity");
        var vets = await veterinarianRepository.GetAllWithUserAsync();
        return View(vets);
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpGet]
    public async Task<IActionResult> CreateVeterinarian()
    {
        SetNav("Tạo bác sĩ thú y", "Veterinarians", "Identity");
        return View(await BuildVeterinarianVmAsync(new VeterinarianUpsertViewModel()));
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVeterinarian(VeterinarianUpsertViewModel model)
    {
        SetNav("Tạo bác sĩ thú y", "Veterinarians", "Identity");
        model = await BuildVeterinarianVmAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await veterinarianRepository.ExistsByUserIdAsync(model.UserId))
        {
            ModelState.AddModelError(nameof(model.UserId), "User này đã được gán veterinarian");
            return View(model);
        }

        var vet = new Veterinarian
        {
            UserId = model.UserId,
            Specialty = model.Specialty?.Trim(),
            LicenseNo = model.LicenseNo?.Trim(),
            YearsOfExperience = model.YearsOfExperience,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        await veterinarianRepository.AddAsync(vet);
        TempData["Success"] = "Tạo veterinarian thành công";
        return RedirectToAction(nameof(Veterinarians));
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpGet]
    public async Task<IActionResult> EditVeterinarian(int id)
    {
        SetNav("Cập nhật bác sĩ thú y", "Veterinarians", "Identity");
        var vet = await veterinarianRepository.GetByIdAsync(id);
        if (vet is null)
        {
            return NotFound();
        }

        var vm = new VeterinarianUpsertViewModel
        {
            Id = vet.Id,
            UserId = vet.UserId,
            Specialty = vet.Specialty,
            LicenseNo = vet.LicenseNo,
            YearsOfExperience = vet.YearsOfExperience,
            Status = vet.Status
        };

        return View(await BuildVeterinarianVmAsync(vm));
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVeterinarian(VeterinarianUpsertViewModel model)
    {
        SetNav("Cập nhật bác sĩ thú y", "Veterinarians", "Identity");
        model = await BuildVeterinarianVmAsync(model);

        if (!ModelState.IsValid || !model.Id.HasValue)
        {
            return View(model);
        }

        var vet = await veterinarianRepository.GetByIdAsync(model.Id.Value);
        if (vet is null)
        {
            return NotFound();
        }

        if (await veterinarianRepository.ExistsByUserIdAsync(model.UserId, vet.Id))
        {
            ModelState.AddModelError(nameof(model.UserId), "User này đã được gán veterinarian");
            return View(model);
        }

        vet.UserId = model.UserId;
        vet.Specialty = model.Specialty?.Trim();
        vet.LicenseNo = model.LicenseNo?.Trim();
        vet.YearsOfExperience = model.YearsOfExperience;
        vet.Status = model.Status;
        vet.UpdatedAt = DateTime.UtcNow;

        await veterinarianRepository.UpdateAsync(vet);
        TempData["Success"] = "Cập nhật veterinarian thành công";
        return RedirectToAction(nameof(Veterinarians));
    }

    private async Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync()
    {
        var roles = await roleRepository.GetAllAsync();
        return roles.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Name} ({x.Status})"
        });
    }

    private async Task<RoleUpsertViewModel> BuildRoleVmAsync(RoleUpsertViewModel model)
    {
        var permissions = await permissionRepository.GetAllAsync();
        model.PermissionOptions = permissions.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Module} - {x.Code}"
        }).ToList();

        return model;
    }

    private async Task<List<Permission>> EnsureCrudPermissionsAsync()
    {
        var permissions = await permissionRepository.GetAllAsync();
        var requiredModules = new[]
        {
            (Module: "users", DisplayName: "Quản lý người dùng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "roles", DisplayName: "Quản lý vai trò", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "permissions", DisplayName: "Phân quyền", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "staff", DisplayName: "Quản lý nhân viên", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "veterinarians", DisplayName: "Quản lý bác sĩ thú y", Codes: new[] { "view", "create", "update", "delete" })
        };

        var hasChanges = false;
        foreach (var module in requiredModules)
        {
            foreach (var action in module.Codes)
            {
                var code = $"{module.Module}.{action}";
                var existing = permissions.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    continue;
                }

                var permission = new Permission
                {
                    Code = code,
                    Name = action switch
                    {
                        "view" => $"Xem {module.DisplayName}",
                        "create" => $"Thêm {module.DisplayName}",
                        "update" => $"Sửa {module.DisplayName}",
                        "delete" => $"Xóa {module.DisplayName}",
                        _ => code
                    },
                    Description = $"Tự tạo quyền {action} cho {module.DisplayName}",
                    Module = module.Module,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await permissionRepository.AddAsync(permission);
                permissions.Add(permission);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            permissions = await permissionRepository.GetAllAsync();
        }

        return permissions;
    }

    private static RolePermissionsViewModel BuildRolePermissionsMatrix(Role role, IEnumerable<Permission> permissions, IEnumerable<int>? selectedPermissionIds = null)
    {
        var selectedSet = selectedPermissionIds?.ToHashSet() ?? role.RolePermissions.Select(x => x.PermissionId).ToHashSet();
        var matrix = new RolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name,
            RoleStatus = role.Status
        };

        var grouped = permissions
            .GroupBy(x => x.Module ?? string.Empty)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in grouped)
        {
            var row = new RolePermissionModuleRowViewModel
            {
                Module = group.Key,
                DisplayName = GetModuleDisplayName(group.Key)
            };

            row.View = BuildCell(group, row.Module, "view", selectedSet);
            row.Create = BuildCell(group, row.Module, "create", selectedSet);
            row.Update = BuildCell(group, row.Module, "update", selectedSet);
            row.Delete = BuildCell(group, row.Module, "delete", selectedSet);

            matrix.Rows.Add(row);
        }

        return matrix;
    }


    private static RolePermissionCellViewModel BuildCell(IGrouping<string, Permission> group, string module, string action, HashSet<int> selectedSet)
    {
        var permission = group.FirstOrDefault(x => string.Equals(x.Code, $"{module}.{action}", StringComparison.OrdinalIgnoreCase));
        return new RolePermissionCellViewModel
        {
            PermissionId = permission?.Id ?? 0,
            Code = permission?.Code ?? $"{module}.{action}",
            Name = permission?.Name ?? string.Empty,
            IsActive = permission?.IsActive ?? false,
            IsGranted = permission is not null && selectedSet.Contains(permission.Id)
        };
    }

    private static string GetModuleDisplayName(string module) => module switch
    {
        "users" => "Người dùng",
        "roles" => "Vai trò",
        "permissions" => "Phân quyền",
        "staff" => "Nhân viên",
        "veterinarians" => "Bác sĩ thú y",
        _ => module
    };

    private async Task<StaffUpsertViewModel> BuildStaffVmAsync(StaffUpsertViewModel model)
    {
        var users = await userRepository.GetAllWithRoleAsync();
        model.UserOptions = users.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Username} - {x.FullName}"
        });

        return model;
    }

    private async Task<VeterinarianUpsertViewModel> BuildVeterinarianVmAsync(VeterinarianUpsertViewModel model)
    {
        var users = await userRepository.GetAllWithRoleAsync();
        model.UserOptions = users.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Username} - {x.FullName}"
        });

        return model;
    }

    private void SetNav(string title, string active, string activeParent)
    {
        ViewData["Title"] = title;
        ViewData["Active"] = active;
        ViewData["ActiveParent"] = activeParent;
    }
}
