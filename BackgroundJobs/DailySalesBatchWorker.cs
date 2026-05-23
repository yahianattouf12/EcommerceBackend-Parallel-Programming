using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceBackend.BackgroundJobs;

public class DailySalesBatchWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IServiceProvider _provider;
    private readonly ILogger<DailySalesBatchWorker> _logger;

    public DailySalesBatchWorker(
        IBackgroundJobQueue queue,
        IServiceProvider provider,
        ILogger<DailySalesBatchWorker> logger)
    {
        _queue = queue;
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailySalesBatchWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var job))
            {
                if (job is DailySalesBatchJob batchJob)
                {
                    await ProcessDailySales(batchJob.Date);
                }
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task ProcessDailySales(DateTime date)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int chunkSize = 5;
        int skip = 0;

        int totalOrders = 0;
        int totalItems = 0;
        decimal totalRevenue = 0;

        while (true)
        {
            var orders = await context.Orders
                .Include(o => o.Items)
                .Where(o => o.CreatedAt.Date == date.Date)
                .OrderBy(o => o.Id)
                .Skip(skip)
                .Take(chunkSize)
                .ToListAsync();

            if (orders.Count == 0)
                break;

            totalOrders += orders.Count;
            totalItems += orders.Sum(o => o.Items.Sum(i => i.Quantity));
            totalRevenue += orders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

            _logger.LogInformation("Processed chunk: {Count} orders", orders.Count);

            skip += chunkSize;
        }

        var report = new DailySalesReport
        {
            Date = date,
            TotalOrders = totalOrders,
            TotalItemsSold = totalItems,
            TotalRevenue = totalRevenue
        };

        context.DailySalesReports.Add(report);
        await context.SaveChangesAsync();

        _logger.LogInformation("Daily sales batch completed for {Date}", date);
    }
}
