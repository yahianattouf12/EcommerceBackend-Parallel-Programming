using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceBackend.BackgroundJobs;

public class DailySalesBatchMThreadsWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IServiceProvider _provider;
    private readonly ILogger<DailySalesBatchMThreadsWorker> _logger;

    public DailySalesBatchMThreadsWorker(
        IBackgroundJobQueue queue,
        IServiceProvider provider,
        ILogger<DailySalesBatchMThreadsWorker> logger)
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
                    try
                    {
                        await ProcessDailySalesParallel(batchJob.Date, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing daily sales batch for {Date}", batchJob.Date);
                    }
                }
            }

            await Task.Delay(500, stoppingToken);
        }

        _logger.LogInformation("DailySalesBatchWorker stopping.");
    }

    private async Task ProcessDailySalesParallel(DateTime date, CancellationToken token)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int chunkSize = 5;

        // 1) احسب عدد الطلبات لهذا اليوم
        var totalOrdersCount = await context.Orders
            .Where(o => o.CreatedAt.Date == date.Date)
            .CountAsync(token);

        if (totalOrdersCount == 0)
        {
            _logger.LogInformation("No orders found for {Date}", date);
            return;
        }

        var totalChunks = (int)Math.Ceiling(totalOrdersCount / (double)chunkSize);

        _logger.LogInformation(
            "Starting parallel daily sales batch for {Date}. TotalOrders={TotalOrders}, ChunkSize={ChunkSize}, TotalChunks={TotalChunks}",
            date, totalOrdersCount, chunkSize, totalChunks);

        // متغيرات مشتركة لتجميع النتائج (Thread-safe)
        int totalOrders = 0;
        int totalItems = 0;
        decimal totalRevenue = 0m;

        object lockObj = new();

        // 2) شغّل كل Chunk في Task منفصل (Parallel)
        var chunkIndexes = Enumerable.Range(0, totalChunks);

        await Parallel.ForEachAsync(chunkIndexes, new ParallelOptions
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = Environment.ProcessorCount // عدد الأنوية
        },
        async (chunkIndex, ct) =>
        {
            var localScope = _provider.CreateScope();
            var localContext = localScope.ServiceProvider.GetRequiredService<AppDbContext>();

            int skip = chunkIndex * chunkSize;

            var orders = await localContext.Orders
                .Include(o => o.Items)
                .Where(o => o.CreatedAt.Date == date.Date)
                .OrderBy(o => o.Id)
                .Skip(skip)
                .Take(chunkSize)
                .ToListAsync(ct);

            if (orders.Count == 0)
                return;

            int chunkOrders = orders.Count;
            int chunkItems = orders.Sum(o => o.Items.Sum(i => i.Quantity));
            decimal chunkRevenue = orders.Sum(o => o.Items.Sum(i => i.Quantity * i.UnitPrice));

            // تجميع النتائج بشكل Thread-safe
            lock (lockObj)
            {
                totalOrders += chunkOrders;
                totalItems += chunkItems;
                totalRevenue += chunkRevenue;
            }

            _logger.LogInformation(
                "Processed chunk {ChunkIndex}/{TotalChunks}: Orders={ChunkOrders}, Items={ChunkItems}, Revenue={ChunkRevenue}",
                chunkIndex + 1, totalChunks, chunkOrders, chunkItems, chunkRevenue);
        });

        // 3) حفظ التقرير النهائي بعد انتهاء كل الـ Chunks
        var finalReport = new DailySalesReport
        {
            Date = date,
            TotalOrders = totalOrders,
            TotalItemsSold = totalItems,
            TotalRevenue = totalRevenue
        };

        context.DailySalesReports.Add(finalReport);
        await context.SaveChangesAsync(token);

        _logger.LogInformation(
            "Parallel daily sales batch completed for {Date}. TotalOrders={TotalOrders}, TotalItems={TotalItems}, TotalRevenue={TotalRevenue}",
            date, totalOrders, totalItems, totalRevenue);
    }
}
