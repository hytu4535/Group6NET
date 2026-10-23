using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.DTOs;
using PetManagementSystem.Models;

namespace PetManagementSystem.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/roles/{roleId}/permissions
        [HttpGet("{roleId}/permissions")]
        public async Task<ActionResult<List<int>>> GetRolePermissions(int roleId)
        {
            try
            {
                var permissionIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                return Ok(permissionIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        // POST: api/roles/update-permissions
        [HttpPost("update-permissions")]
        public async Task<ActionResult> UpdateRolePermissions([FromBody] RolePermissionUpdateDto dto)
        {
            if (dto == null || dto.RoleId <= 0)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                // Kiểm tra role tồn tại (tùy chọn)
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == dto.RoleId);
                if (!roleExists)
                    return NotFound(new { success = false, message = "Role không tồn tại." });

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1️⃣ Xóa hết quyền cũ của role này
                    var existing = _context.RolePermissions.Where(rp => rp.RoleId == dto.RoleId);
                    _context.RolePermissions.RemoveRange(existing);
                    await _context.SaveChangesAsync();

                    // 2️⃣ Thêm quyền mới (nếu có)
                    if (dto.PermissionIds.Any())
                    {
                        var newRps = dto.PermissionIds.Select(pid => new RolePermission
                        {
                            RoleId = dto.RoleId,
                            PermissionId = pid
                        }).ToList();

                        await _context.RolePermissions.AddRangeAsync(newRps);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return Ok(new { success = true, message = "Cập nhật phân quyền thành công." });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // sẽ bị bắt bởi block catch bên ngoài
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}