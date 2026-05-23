using ECommerceBackend.BackgroundJobs;
using ECommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISalesReportService _salesReportService;
    private readonly IBackgroundJobQueue _backgroundJobQueue;


    public ReportsController(ISalesReportService salesReportService, IBackgroundJobQueue backgroundJobQueue)
    {
        _salesReportService = salesReportService;
        _backgroundJobQueue = backgroundJobQueue;
    }


    //! ////////////////////////////////////////////////// !//
    //?    ...this is hte 4th question in the project...   ?//
    //! ////////////////////////////////////////////////// !//

    // BEFORE Batch Processing
    [HttpPost("daily-report-sync")]
    public async Task<IActionResult> GenerateDailyReportSync()
    {
        var date = DateTime.UtcNow.Date;

        var report = await _salesReportService.GenerateDailyReportSync(date);

        return Ok(report);
    }

    [HttpPost("daily-report-batch")]
    public IActionResult GenerateDailyReportBatch()
    {
        var date = DateTime.UtcNow.Date;

        _backgroundJobQueue.Enqueue(new DailySalesBatchJob(date));

        return Ok("Batch job started");
    }

    [HttpPost("daily-report-batch-mthreads")]
    public IActionResult GenerateDailyReportBatchMThreads()
    {
        var date = DateTime.UtcNow.Date;

        _backgroundJobQueue.Enqueue(new DailySalesBatchJob(date));

        return Ok("Batch job started");
    }
}
