namespace PetManagementSystem.Models;

public class ImportDetail
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal Subtotal { get; set; }

    public ImportReceipt? Receipt { get; set; }
    public Product? Product { get; set; }
}
