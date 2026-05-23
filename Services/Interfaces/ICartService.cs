using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface ICartService
{
    Task<IEnumerable<CartItem>> GetItemsAsync(int userId);
    Task<CartItem> AddItemAsync(int userId, int productId, int quantity);
    Task<CartItem?> UpdateQuantityAsync(int userId, int productId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int productId);
    Task<bool> ClearCartAsync(int userId);
}
