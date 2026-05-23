using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface INotificationService
{
    Task SendOrderCreatedAsync(Order order, Invoice invoice);
}
