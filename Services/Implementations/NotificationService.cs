using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceBackend.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendOrderCreatedAsync(Order order, Invoice invoice)
    {
        _logger.LogInformation(
            "Notification: Order {OrderId} created with Invoice {InvoiceId}, Total = {Total}",
            order.Id,
            invoice.Id,
            invoice.TotalAmount);

        // ممكن لاحقًا تستبدلها بإرسال Email / SMS
        
        for(int i = 0; i < 100000; i++) {}

        return Task.CompletedTask;
    }
}
