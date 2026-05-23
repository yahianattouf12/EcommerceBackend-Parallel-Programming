namespace ECommerceBackend.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public List<CartItem> CartItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}
