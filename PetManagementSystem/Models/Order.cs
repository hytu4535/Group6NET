namespace PetManagementSystem.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    /// <summary>Họ tên người nhận hàng (chụp lại tại thời điểm đặt, không đổi theo hồ sơ về sau).</summary>
    public string? ReceiverName { get; set; }
    /// <summary>Email nhận thông tin đơn hàng.</summary>
    public string? ReceiverEmail { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "pending";
    public string? Note { get; set; }

    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Invoice? Invoice { get; set; }
}
