using Microsoft.AspNetCore.Mvc;
using PetManagementSystem.Models;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace PetManagementSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly PetManagementSystem.Data.AppDbContext _dbContext;

        public CartController(PetManagementSystem.Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Get active cart for the user
            var cart = _dbContext.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .FirstOrDefault();

            if (cart == null)
            {
                // Create a new cart if none exists
                cart = new Cart { UserId = userId, Status = "active", CreatedAt = DateTime.UtcNow };
                _dbContext.Carts.Add(cart);
                _dbContext.SaveChanges();
            }

            var cartItems = _dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Product)
                .ToList();

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var cartItem = _dbContext.CartItems.Find(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Verify that the cart item belongs to the current user's cart
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == userId && c.Status == "active");

            if (cart == null || cartItem.CartId != cart.Id)
            {
                return Forbid();
            }

            cartItem.Quantity = quantity;
            _dbContext.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cartItem = _dbContext.CartItems.Find(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Verify ownership
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == userId && c.Status == "active");

            if (cart == null || cartItem.CartId != cart.Id)
            {
                return Forbid();
            }

            _dbContext.CartItems.Remove(cartItem);
            _dbContext.SaveChanges();

            return Ok(new { success = true });
        }

        public IActionResult Checkout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Get active cart for the user
            var cart = _dbContext.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .FirstOrDefault();

            if (cart == null)
            {
                return RedirectToAction("Index");
            }

            var cartItems = _dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn trống.";
                return RedirectToAction("Index");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = "Chưa cung cấp", // In real app, collect from user
                Phone = "Chưa cung cấp",
                TotalAmount = 0,
                Status = "pending",
                Note = "Đơn hàng từ giỏ hàng"
            };

            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            // Add order items
            decimal total = 0;
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product?.SellPrice ?? 0
                };
                _dbContext.OrderItems.Add(orderItem);
                total += (item.Product?.SellPrice ?? 0) * item.Quantity;
            }

            order.TotalAmount = total;
            _dbContext.SaveChanges();

            // Clear cart
            _dbContext.CartItems.RemoveRange(cartItems);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("MyOrders", "Account");
        }
    }
}