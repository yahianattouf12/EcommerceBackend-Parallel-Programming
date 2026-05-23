using System.Text.Json.Serialization;

namespace ECommerceBackend.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [JsonIgnore]
    public List<OrderItem> Items { get; set; } = new();
}
