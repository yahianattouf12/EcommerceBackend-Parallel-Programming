namespace ECommerceBackend.BackgroundJobs;

public class OrderBackgroundJob : BackgroundJob
{
    public int OrderId { get; }

    public OrderBackgroundJob(int orderId)
    {
        OrderId = orderId;
    }
}

