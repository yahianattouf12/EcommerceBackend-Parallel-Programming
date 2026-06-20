using ECommerceBackend.AOP;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private static readonly object _stockLock = new();
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3);
    private static int _activeThreads = 0;
    private readonly IServiceProvider _serviceProvider;


    public ProductService(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    [Cache(60)] 
    public async Task<List<Product>> GetAllProductsAsyncWithCache()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(int id, Product updated)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            throw new Exception("Product not found");

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Price = updated.Price;
        product.StockQuantity = updated.StockQuantity;

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DecreaseQuantityAsync(int productId, int quantity)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            throw new Exception("Product not found");

        product.StockQuantity -= quantity;

        await _context.SaveChangesAsync();
    }

    public async Task DecreaseQuantitySafeAsync(int productId, int qty)
    {
        lock (_stockLock)
        {
            using var transaction = _context.Database.BeginTransaction();

            var product = _context.Products
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            if (product.StockQuantity < qty)
                throw new Exception("Not enough stock");

            product.StockQuantity -= qty;

            _context.SaveChanges();

            transaction.Commit();
        }
    }

     // يتم التحكم في عدد الثريدات يلي لح تفوت وتعدل (نفس الطلب يلي قبل بس مع تحكم بعدد الثريدات)
    public async Task DecreaseQuantityControlledAsync(int productId, int qty)
    {
        await _semaphore.WaitAsync();
        Interlocked.Increment(ref _activeThreads);
        Console.WriteLine($"-> Thread ENTERED | Active Threads = {_activeThreads}");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using var transaction = context.Database.BeginTransaction();

            var product = context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            if (product.StockQuantity < qty)
                throw new Exception("Not enough stock");


            await Task.Delay(3000);
            product.StockQuantity -= qty;

            context.SaveChanges();

            transaction.Commit();
        }
        finally
        {
            Interlocked.Decrement(ref _activeThreads);
            Console.WriteLine($"<- Thread EXITED | Active Threads = {_activeThreads}");
            _semaphore.Release(); 
        }
    }

}
