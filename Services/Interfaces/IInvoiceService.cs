using ECommerceBackend.Models;

namespace ECommerceBackend.Services.Interfaces;

public interface IInvoiceService
{
    Task<Invoice> GenerateInvoiceAsync(int orderId);
}
