namespace PetManagementSystem.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "pending";
    public string? TransactionCode { get; set; }

    public Order? Order { get; set; }
}
