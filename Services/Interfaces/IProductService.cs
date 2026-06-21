using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
    Task DecreaseQuantityAsync(int productId, int quantity);

    Task DecreaseQuantitySafeAsync(int productId, int qty);
    Task DecreaseQuantityControlledAsync(int productId, int qty);

    Task<List<Product>> GetAllProductsAsyncWithCache();

    Task<Product> UpdateStockOptimisticAsync(int productId, int quantity);

    Task<Product> UpdateStockDistributedAsync(int productId, int quantity);

}
