namespace PetManagementSystem.Models;

public class Supplier
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }

    public ICollection<ImportReceipt> ImportReceipts { get; set; } = new List<ImportReceipt>();
}
