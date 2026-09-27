namespace PetManagementSystem.ViewModels.Commerce;

public class CartRowViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CartListViewModel : PagedListViewModel<CartRowViewModel>
{
    public string? Search { get; set; }
    public string? Status { get; set; }
}

public class CartItemRowViewModel
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CartStatus { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal => Quantity * Price;
}

public class CartDetailsViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemRowViewModel> Items { get; set; } = [];
    public decimal TotalAmount => Items.Sum(x => x.Subtotal);
}

public class CartItemListViewModel : PagedListViewModel<CartItemRowViewModel>
{
    public int? CartId { get; set; }
    public string? Search { get; set; }
}
