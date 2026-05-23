using System.Text.Json.Serialization;

namespace ECommerceBackend.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    // Relations
    [JsonIgnore]
    public List<CartItem> CartItems { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
}
