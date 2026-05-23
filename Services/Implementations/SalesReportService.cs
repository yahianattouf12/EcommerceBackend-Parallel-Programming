using ECommerceBackend.Data;
using Microsoft.EntityFrameworkCore;

public class SalesReportService : ISalesReportService
{
    private readonly AppDbContext _context;

    public SalesReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DailySalesReport> GenerateDailyReportSync(DateTime date)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.CreatedAt.Date == date.Date)
            .ToListAsync();

        var report = new DailySalesReport
        {
            Date = date,
            TotalOrders = orders.Count,
            TotalItemsSold = orders.Sum(o => o.Items.Sum(i => i.Quantity)),
            TotalRevenue = orders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice))
        };

        _context.DailySalesReports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }
}
