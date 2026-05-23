using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBackend.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> GenerateInvoiceAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found.");

        var total = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        var invoice = new Invoice
        {
            OrderId = order.Id,
            TotalAmount = total
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }
}
