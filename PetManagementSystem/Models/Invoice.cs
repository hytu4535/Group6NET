namespace PetManagementSystem.Models;

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public int? IssuedBy { get; set; }

    public Order? Order { get; set; }
    public User? IssuedByUser { get; set; }
}
