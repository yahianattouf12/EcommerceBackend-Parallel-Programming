using ECommerceBackend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceBackend.BackgroundJobs;

public class OrderBackgroundWorker : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderBackgroundWorker> _logger;

    //todo : hard code you should to replace it with some logic
    private const int UserId = 2;

    public OrderBackgroundWorker(
        IBackgroundJobQueue queue,
        IServiceProvider serviceProvider,
        ILogger<OrderBackgroundWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderBackgroundWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            
                if (_queue.TryDequeue(out var job))
                {
                    if (job is OrderBackgroundJob orderJob)
                    {
                       using var scope = _serviceProvider.CreateScope();

                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var order = await orderService.GetOrderByIdAsync(orderJob.OrderId, UserId); //Todo: hard code fix that..
                        if (order == null)
                        {
                            _logger.LogWarning("Order {OrderId} not found for background job.", orderJob.OrderId);
                            continue;
                        }

                        var invoice = await invoiceService.GenerateInvoiceAsync(order.Id);
                        await notificationService.SendOrderCreatedAsync(order, invoice);

                        _logger.LogInformation(
                            "Background job processed for Order {OrderId}, Invoice {InvoiceId}",
                            order.Id,
                            invoice.Id);
                    }
                }
                else
                {
                    await Task.Delay(500, stoppingToken);
                }
            
        }

        _logger.LogInformation("OrderBackgroundWorker stopping.");
    }
}
