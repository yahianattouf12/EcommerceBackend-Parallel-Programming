using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services.Implementations;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CartItem>> GetItemsAsync(int userId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();
    }

    public async Task<CartItem> AddItemAsync(int userId, int productId, int quantity)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (existing != null)
        {
            existing.Quantity += quantity;
            await _context.SaveChangesAsync();
            return existing;
        }

        var item = new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity
        };

        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<CartItem?> UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (item == null)
            return null;

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> RemoveItemAsync(int userId, int productId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (item == null)
            return false;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var items = _context.CartItems.Where(ci => ci.UserId == userId);

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return true;
    }
}
