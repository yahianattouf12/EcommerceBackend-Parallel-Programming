namespace ECommerceBackend.Models;

public enum PaymentResultStatus
{
    Success,
    Failed
}

public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentResultStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}