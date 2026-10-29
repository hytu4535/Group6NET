namespace PetManagementSystem.DTOs;

public class RolePermissionUpdateDto
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = new List<int>();
}