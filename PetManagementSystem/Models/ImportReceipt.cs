namespace PetManagementSystem.Models;

public class ImportReceipt
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int? EmployeeId { get; set; }
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }

    public Supplier? Supplier { get; set; }
    public User? Employee { get; set; }
    public ICollection<ImportDetail> Details { get; set; } = new List<ImportDetail>();
}
