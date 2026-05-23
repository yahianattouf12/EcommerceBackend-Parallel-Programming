using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(int userId)
    {
        // 1) اجلب عناصر السلة
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new Exception("Cart is empty");

        // 2) احسب الإجمالي
        decimal total = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);

        // 3) أنشئ الطلب
        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 4) أنشئ OrderItems
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            };

            _context.OrderItems.Add(orderItem);
        }

        // 5) احذف عناصر السلة
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId, int userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
    }
}
