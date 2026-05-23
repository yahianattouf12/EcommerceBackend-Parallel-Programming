using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(int userId);
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    Task<Order?> GetOrderByIdAsync(int orderId, int userId);
}
