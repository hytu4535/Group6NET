using Microsoft.AspNetCore.Authorization;
// Test comment to verify Edit works
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
    public async Task<IActionResult> Users(string? search, int? roleId, string? status = "all", int page = 1, int pageSize = 10)
    {
        SetNav("Quản lý người dùng", "Users", "Identity");
        var users = await userRepository.GetAllWithRoleAsync(includeInactive: true);
        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            users = users.Where(u =>
                u.Username.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                (u.FullName?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.Email?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.PhoneNumber?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        if (roleId.HasValue) users = users.Where(u => u.RoleId == roleId.Value).ToList();
        if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            users = users.Where(u => !string.Equals(u.Status, "deleted", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            users = users.Where(u => u.Status == status).ToList();
        }
        ViewData["Search"] = search;
        ViewData["RoleId"] = roleId;
        ViewData["Status"] = status;
        ViewData["RoleOptions"] = (await roleRepository.GetAllAsync(includeInactive: false));
        SetPagination(users.Count, ref page, pageSize);
        ViewData["PageItems"] = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
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

    [Authorize(Policy = "users.view")]
    [HttpGet]
    public async Task<IActionResult> DetailsUser(int id)
    {
        SetNav("Chi tiết người dùng", "Users", "Identity");
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(user);
    }

    [Authorize(Policy = "users.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockUser(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.Equals(user.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Info"] = "Không thể mở khóa tài khoản đã xóa mềm.";
            return RedirectToAction(nameof(Users));
        }

        var isActive = string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase);
        user.Status = isActive ? "inactive" : "active";
        user.UpdatedAt = DateTime.UtcNow;

        if (user.Staff is not null)
        {
            user.Staff.Status = user.Status;
            user.Staff.UpdatedAt = DateTime.UtcNow;
        }

        if (user.Veterinarian is not null)
        {
            user.Veterinarian.Status = user.Status;
            user.Veterinarian.UpdatedAt = DateTime.UtcNow;
        }

        await userRepository.UpdateAsync(user);
        TempData["Success"] = isActive
            ? "Đã khóa người dùng. Tài khoản không thể đăng nhập."
            : "Đã mở khóa người dùng. Tài khoản có thể đăng nhập lại.";
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Policy = "users.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDeleteUser(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.Equals(user.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Info"] = "Người dùng đã được xóa mềm trước đó";
            return RedirectToAction(nameof(Users));
        }

        if (await userRepository.HasRelatedDataAsync(id))
        {
            user.Status = "deleted";
            user.UpdatedAt = DateTime.UtcNow;
            if (user.Staff is not null)
            {
                user.Staff.Status = "deleted";
                user.Staff.UpdatedAt = DateTime.UtcNow;
            }
            if (user.Veterinarian is not null)
            {
                user.Veterinarian.Status = "deleted";
                user.Veterinarian.UpdatedAt = DateTime.UtcNow;
            }

            await userRepository.UpdateAsync(user);
            TempData["Success"] = "Đã xóa mềm người dùng vì tài khoản đã phát sinh dữ liệu";
        }
        else
        {
            await userRepository.DeleteHardAsync(user);
            TempData["Success"] = "Đã xóa cứng người dùng vì tài khoản chưa phát sinh dữ liệu";
        }
        return RedirectToAction(nameof(Users));
    }

[Authorize(Policy = "roles.view")]
    public async Task<IActionResult> Roles(string? search, string? status = "active", int page = 1, int pageSize = 10)
    {
        SetNav("Quản lý vai trò", "Roles", "Identity");
        var roles = await roleRepository.GetAllAsync(includeInactive: true);
        if (!string.IsNullOrWhiteSpace(search))
        {
            roles = roles.Where(r => r.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                (r.Description?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase)) roles = roles.Where(r => r.Status == status).ToList();
        ViewData["Search"] = search;
        ViewData["Status"] = status;
        SetPagination(roles.Count, ref page, pageSize);
        ViewData["PageItems"] = roles.Skip((page - 1) * pageSize).Take(pageSize).ToList();
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

    [Authorize(Policy = "roles.view")]
    [HttpGet]
    public async Task<IActionResult> DetailsRole(int id)
    {
        SetNav("Chi tiết vai trò", "Roles", "Identity");
        var role = await roleRepository.GetRoleWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var permissions = await EnsureCrudPermissionsAsync();
        var model = new RoleDetailsViewModel
        {
            Role = role,
            PermissionMatrix = BuildRolePermissionsMatrix(role, permissions)
        };

        return View(model);
    }

    [Authorize(Policy = "roles.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockRole(int id)
    {
        var role = await roleRepository.GetRoleWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        // Check if role is assigned to any user
        var users = await userRepository.GetAllWithRoleAsync();
        if (users.Any(u => u.RoleId == id))
        {
            TempData["Error"] = "Không thể khóa vai trò vì đang được gán cho người dùng";
            return RedirectToAction(nameof(Roles));
        }

        if (string.Equals(role.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            role.Status = "inactive";
            TempData["Success"] = "Đã khóa vai trò";
        }
        else
        {
            role.Status = "active";
            TempData["Success"] = "Đã mở khóa vai trò";
        }

        role.UpdatedAt = DateTime.UtcNow;
        await roleRepository.UpdateAsync(role, role.RolePermissions.Select(x => x.PermissionId));
        return RedirectToAction(nameof(Roles));
    }

    [Authorize(Policy = "roles.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDeleteRole(int id)
    {
        var role = await roleRepository.GetRoleWithPermissionsAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        // Check if role is assigned to any user
        var users = await userRepository.GetAllWithRoleAsync();
        if (users.Any(u => u.RoleId == id))
        {
            TempData["Error"] = "Không thể xóa vai trò vì đang được gán cho người dùng";
            return RedirectToAction(nameof(Roles));
        }

        if (string.Equals(role.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Info"] = "Vai trò đã được xóa mềm trước đó";
            return RedirectToAction(nameof(Roles));
        }

        role.Status = "deleted";
        role.UpdatedAt = DateTime.UtcNow;
        await roleRepository.UpdateAsync(role, role.RolePermissions.Select(x => x.PermissionId));
        TempData["Success"] = "Đã xóa mềm vai trò";
        return RedirectToAction(nameof(Roles));
    }

    [Authorize(Policy = "permissions.view")]
    public async Task<IActionResult> Permissions(int page = 1, int pageSize = 10)
    {
        SetNav("Phân quyền", "Permissions", "Identity");
        var permissions = await permissionRepository.GetAllAsync();
        SetPagination(permissions.Count, ref page, pageSize);
        ViewData["PageItems"] = permissions.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return View(permissions);
    }

    [Authorize(Policy = "permissions.update")]
    [HttpGet]
    public async Task<IActionResult> RolePermissions(int id)
    {
        SetNav("Phân quyền theo vai trò", "Permissions", "Identity");

        System.Diagnostics.Debug.WriteLine($"Loading role permissions for role ID: {id}");
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
    public async Task<IActionResult> Staff(string? search, string? position, DateTime? hireDateFrom, DateTime? hireDateTo, string? status = "all", int page = 1, int pageSize = 10)
    {
        SetNav("Quản lý nhân viên", "Staff", "Identity");
        var staffList = await staffRepository.GetAllWithUserAsync(includeInactive: true);
        if (!string.IsNullOrWhiteSpace(search))
        {
            staffList = staffList.Where(s => (s.User?.Username?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.User?.FullName?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(position)) staffList = staffList.Where(s => s.Position?.Contains(position.Trim(), StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (hireDateFrom.HasValue) staffList = staffList.Where(s => s.HireDate >= hireDateFrom).ToList();
        if (hireDateTo.HasValue) staffList = staffList.Where(s => s.HireDate <= hireDateTo.Value.Date.AddDays(1).AddTicks(-1)).ToList();
        if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            staffList = staffList.Where(s => !string.Equals(s.Status, "deleted", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(s.User?.Status, "deleted", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (status.Equals("deleted", StringComparison.OrdinalIgnoreCase))
        {
            staffList = staffList.Where(s => string.Equals(s.Status, "deleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.User?.Status, "deleted", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            staffList = staffList.Where(s => s.Status == status && s.User?.Status == status).ToList();
        }
        ViewData["Search"] = search;
        ViewData["Position"] = position;
        ViewData["HireDateFrom"] = hireDateFrom?.ToString("yyyy-MM-dd");
        ViewData["HireDateTo"] = hireDateTo?.ToString("yyyy-MM-dd");
        ViewData["Status"] = status;
        SetPagination(staffList.Count, ref page, pageSize);
        ViewData["PageItems"] = staffList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
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

        ViewData["LinkedUsername"] = staff.User?.Username ?? string.Empty;

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

        model.UserId = staff.UserId;

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

    [Authorize(Policy = "staff.view")]
    [HttpGet]
    public async Task<IActionResult> DetailsStaff(int id)
    {
        SetNav("Chi tiết nhân viên", "Staff", "Identity");
        var staff = await staffRepository.GetByIdAsync(id);
        if (staff is null)
        {
            return NotFound();
        }

        return View(staff);
    }

    [Authorize(Policy = "staff.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockStaff(int id)
    {
        var staff = await staffRepository.GetByIdAsync(id);
        if (staff is null)
        {
            return NotFound();
        }

        if (string.Equals(staff.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            staff.Status = "inactive";
            TempData["Success"] = "Đã khóa nhân viên";
        }
        else
        {
            staff.Status = "active";
            TempData["Success"] = "Đã mở khóa nhân viên";
        }

        staff.UpdatedAt = DateTime.UtcNow;
        if (staff.User is not null)
        {
            staff.User.Status = staff.Status;
            staff.User.UpdatedAt = DateTime.UtcNow;
        }
        await staffRepository.UpdateAsync(staff);
        return RedirectToAction(nameof(Staff));
    }

    [Authorize(Policy = "staff.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDeleteStaff(int id)
    {
        var staff = await staffRepository.GetByIdAsync(id);
        if (staff is null)
        {
            return NotFound();
        }

        if (string.Equals(staff.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Info"] = "Nhân viên đã được xóa mềm trước đó";
            return RedirectToAction(nameof(Staff));
        }

        staff.Status = "deleted";
        staff.UpdatedAt = DateTime.UtcNow;
        if (await userRepository.HasRelatedDataAsync(staff.UserId))
        {
            if (staff.User is not null)
            {
                staff.User.Status = "deleted";
                staff.User.UpdatedAt = DateTime.UtcNow;
            }
            await staffRepository.UpdateAsync(staff);
            TempData["Success"] = "Đã xóa mềm nhân viên vì tài khoản đã phát sinh dữ liệu";
        }
        else if (staff.User is not null)
        {
            await userRepository.DeleteHardAsync(staff.User);
            TempData["Success"] = "Đã xóa cứng nhân viên vì tài khoản chưa phát sinh dữ liệu";
        }
        return RedirectToAction(nameof(Staff));
    }

[Authorize(Policy = "veterinarians.view")]
    public async Task<IActionResult> Veterinarians(string? search, string? specialty, string? licenseNo, int? minExperience, int? maxExperience, string? status = "all", int page = 1, int pageSize = 10)
    {
        SetNav("Quản lý bác sĩ thú y", "Veterinarians", "Identity");
        var vets = await veterinarianRepository.GetAllWithUserAsync(includeInactive: true);
        if (!string.IsNullOrWhiteSpace(search))
        {
            vets = vets.Where(v => (v.User?.Username?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.User?.FullName?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(specialty)) vets = vets.Where(v => v.Specialty?.Contains(specialty.Trim(), StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (!string.IsNullOrWhiteSpace(licenseNo)) vets = vets.Where(v => v.LicenseNo?.Contains(licenseNo.Trim(), StringComparison.OrdinalIgnoreCase) == true).ToList();
        if (minExperience.HasValue) vets = vets.Where(v => v.YearsOfExperience >= minExperience).ToList();
        if (maxExperience.HasValue) vets = vets.Where(v => v.YearsOfExperience <= maxExperience).ToList();
        if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            vets = vets.Where(v => !string.Equals(v.Status, "deleted", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(v.User?.Status, "deleted", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (status.Equals("deleted", StringComparison.OrdinalIgnoreCase))
        {
            vets = vets.Where(v => string.Equals(v.Status, "deleted", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(v.User?.Status, "deleted", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            vets = vets.Where(v => v.Status == status && v.User?.Status == status).ToList();
        }
        ViewData["Search"] = search;
        ViewData["Specialty"] = specialty;
        ViewData["LicenseNo"] = licenseNo;
        ViewData["MinExperience"] = minExperience;
        ViewData["MaxExperience"] = maxExperience;
        ViewData["Status"] = status;
        SetPagination(vets.Count, ref page, pageSize);
        ViewData["PageItems"] = vets.Skip((page - 1) * pageSize).Take(pageSize).ToList();
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

        ViewData["LinkedUsername"] = vet.User?.Username ?? string.Empty;

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

        model.UserId = vet.UserId;

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
        foreach (var permission in permissions)
        {
            var separatorIndex = permission.Code.IndexOf('.');
            if (separatorIndex > 0)
            {
                permission.Module = permission.Code[..separatorIndex];
            }
        }

        var requiredModules = new[]
        {
            (Module: "users", DisplayName: "Quản lý người dùng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "roles", DisplayName: "Quản lý vai trò", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "permissions", DisplayName: "Phân quyền", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "staff", DisplayName: "Quản lý nhân viên", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "veterinarians", DisplayName: "Quản lý bác sĩ thú y", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "categories", DisplayName: "Danh mục sản phẩm", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "suppliers", DisplayName: "Nhà cung cấp", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "products", DisplayName: "Sản phẩm", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "import_receipts", DisplayName: "Phiếu nhập kho", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "carts", DisplayName: "Giỏ hàng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "orders", DisplayName: "Đơn hàng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "payments", DisplayName: "Thanh toán", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "invoices", DisplayName: "Hóa đơn", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "pets", DisplayName: "Thú cưng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "appointments", DisplayName: "Lịch hẹn", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "services", DisplayName: "Dịch vụ", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "service_packages", DisplayName: "Gói dịch vụ", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "pet_packages", DisplayName: "Gói dịch vụ thú cưng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "pet_images", DisplayName: "Hình ảnh thú cưng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "pet_health_records", DisplayName: "Hồ sơ sức khỏe", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "vaccination_records", DisplayName: "Lịch sử tiêm phòng", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "medical_prescriptions", DisplayName: "Đơn thuốc", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "feedbacks", DisplayName: "Phản hồi", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "notifications", DisplayName: "Thông báo", Codes: new[] { "view", "create", "update", "delete" }),
            (Module: "chats", DisplayName: "Chat", Codes: new[] { "view", "create", "update", "delete" })
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
                    if (!string.Equals(existing.Module, module.Module, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.Module = module.Module;
                        existing.UpdatedAt = DateTime.UtcNow;
                        await permissionRepository.UpdateAsync(existing);
                    }
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

    private RolePermissionsViewModel BuildRolePermissionsMatrix(Role role, List<Permission> permissions)
    {
        var selectedSet = role.RolePermissions.Select(x => x.PermissionId).ToHashSet();
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
        "categories" => "Danh mục sản phẩm",
        "suppliers" => "Nhà cung cấp",
        "products" => "Sản phẩm",
        "import_receipts" => "Phiếu nhập kho",
        "carts" => "Giỏ hàng",
        "orders" => "Đơn hàng",
        "payments" => "Thanh toán",
        "invoices" => "Hóa đơn",
        "pets" => "Thú cưng",
        "appointments" => "Lịch hẹn",
        "services" => "Dịch vụ",
        "service_packages" => "Gói dịch vụ",
        "pet_packages" => "Gói dịch vụ thú cưng",
        "pet_images" => "Hình ảnh thú cưng",
        "pet_health_records" => "Hồ sơ sức khỏe",
        "vaccination_records" => "Lịch sử tiêm phòng",
        "medical_prescriptions" => "Đơn thuốc",
        "feedbacks" => "Phản hồi",
        "notifications" => "Thông báo",
        "chats" => "Chat",
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

    [Authorize(Policy = "veterinarians.view")]
    [HttpGet]
    public async Task<IActionResult> DetailsVeterinarian(int id)
    {
        SetNav("Chi tiết bác sĩ thú y", "Veterinarians", "Identity");
        var vet = await veterinarianRepository.GetByIdAsync(id);
        if (vet is null)
        {
            return NotFound();
        }

        return View(vet);
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockVeterinarian(int id)
    {
        var vet = await veterinarianRepository.GetByIdAsync(id);
        if (vet is null)
        {
            return NotFound();
        }

        if (string.Equals(vet.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            vet.Status = "inactive";
            TempData["Success"] = "Đã khóa bác sĩ thú y";
        }
        else
        {
            vet.Status = "active";
            TempData["Success"] = "Đã mở khóa bác sĩ thú y";
        }

        vet.UpdatedAt = DateTime.UtcNow;
        if (vet.User is not null)
        {
            vet.User.Status = vet.Status;
            vet.User.UpdatedAt = DateTime.UtcNow;
        }
        await veterinarianRepository.UpdateAsync(vet);
        return RedirectToAction(nameof(Veterinarians));
    }

    [Authorize(Policy = "veterinarians.update")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDeleteVeterinarian(int id)
    {
        var vet = await veterinarianRepository.GetByIdAsync(id);
        if (vet is null)
        {
            return NotFound();
        }

        if (string.Equals(vet.Status, "deleted", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Info"] = "Bác sĩ thú y đã được xóa mềm trước đó";
            return RedirectToAction(nameof(Veterinarians));
        }

        vet.Status = "deleted";
        vet.UpdatedAt = DateTime.UtcNow;
        if (await userRepository.HasRelatedDataAsync(vet.UserId))
        {
            if (vet.User is not null)
            {
                vet.User.Status = "deleted";
                vet.User.UpdatedAt = DateTime.UtcNow;
            }
            await veterinarianRepository.UpdateAsync(vet);
            TempData["Success"] = "Đã xóa mềm bác sĩ thú y vì tài khoản đã phát sinh dữ liệu";
        }
        else if (vet.User is not null)
        {
            await userRepository.DeleteHardAsync(vet.User);
            TempData["Success"] = "Đã xóa cứng bác sĩ thú y vì tài khoản chưa phát sinh dữ liệu";
        }
        return RedirectToAction(nameof(Veterinarians));
    }

    private void SetNav(string title, string active, string activeParent)
    {
        ViewData["Title"] = title;
        ViewData["Active"] = active;
        ViewData["ActiveParent"] = activeParent;
    }

    private void SetPagination(int totalItems, ref int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalPages"] = totalPages;
    }
}
